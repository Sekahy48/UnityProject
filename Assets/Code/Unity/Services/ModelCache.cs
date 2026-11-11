using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace Unity.Services
{
    /// <summary>
    /// Carga modelos glTF binarios desde la carpeta de datos y los instancia en la escena.
    ///
    /// Mismo papel que <see cref="TextureCache"/> y por la misma razon: los modelos los
    /// nombra el catalogo exportado desde Stack&amp;Go, no el proyecto, asi que no pueden ser
    /// prefabs ni referencias de inspector. Llegan como una ruta relativa y hay que leerlos
    /// de disco en tiempo de ejecucion.
    ///
    /// Lo que se guarda en cache es el <see cref="GltfImport"/>, no el GameObject. Un
    /// GltfImport es el fichero ya interpretado —mallas, materiales, texturas en memoria— y
    /// sabe instanciarse cuantas veces haga falta. Cachear el GameObject en su lugar
    /// obligaria a clonarlo, y clonar una jerarquia entera es mas caro y mas fragil que
    /// pedirle otra instancia a quien ya tiene los datos.
    /// </summary>
    public class ModelCache
    {
        private static ModelCache _instance;
        public static ModelCache Instance => _instance ??= new ModelCache();

        /// <summary>
        /// Se cachea la tarea y no el resultado a proposito.
        ///
        /// Dos entidades que aparecen en el mismo fotograma piden el mismo modelo antes de
        /// que la primera carga termine. Guardando el resultado, la segunda encontraria la
        /// cache vacia y volveria a leer y parsear los mismos megabytes; guardando la tarea,
        /// se engancha a la que ya esta en marcha.
        /// </summary>
        private readonly Dictionary<string, Task<GltfImport>> _imports = new Dictionary<string, Task<GltfImport>>();

        /// <summary>
        /// Instancia un modelo en la escena.
        /// </summary>
        /// <param name="relativePath">Ruta tal como la nombra el catalogo, por ejemplo
        /// <c>models/136_arcon_pequeno_00.glb</c></param>
        /// <param name="parent">Transform bajo el que colgarlo, o null para dejarlo en la raiz</param>
        /// <returns>Raiz unica del modelo instanciado, o null si no se pudo cargar. Quien
        /// llama es dueno de ese objeto: moverlo, desactivarlo o destruirlo no afecta ni a
        /// la cache ni a otras instancias del mismo modelo.</returns>
        public async Task<GameObject> Instantiate(string relativePath, Transform parent = null)
        {
            // Si el que pide el modelo muere mientras carga —una manzana recogida antes de
            // que aparezca—, su transform queda destruido y colgarle nada de el revienta.
            // Se anota si habia padre al empezar para poder distinguir despues "me pidieron
            // la raiz de la escena" de "el padre ya no esta".
            bool expectsParent = parent != null;

            GltfImport import = await Get(relativePath);
            if (import == null) return null;

            if (expectsParent && parent == null)
            {
                Debug.Log($"ModelCache: se descarta {relativePath}, quien lo pidio ya no existe");
                return null;
            }

            // SceneObjectCreation.Always es la clave de que este metodo pueda prometer una
            // raiz. Por defecto glTFast usa WhenMultipleRootNodes: si la escena del fichero
            // tiene un solo nodo raiz lo cuelga directamente del padre, y si tiene varios
            // crea un objeto contenedor. Es decir, la forma de lo instanciado dependeria de
            // como estuviera montado el .glb, y quien llama no podria saber si lo que acaba
            // de aparecer bajo su transform es un objeto o cinco.
            //
            // Always es ademas lo que hace que un parent nulo sea legitimo. La rama que
            // glTFast toma cuando NO crea objeto de escena hace 'm_Parent.gameObject', que
            // con null revienta; la que si lo crea hace 'SetParent(m_Parent, false)', y eso
            // con null significa dejarlo en la raiz de la escena. Forzando Always nunca se
            // entra en la primera.
            GameObjectInstantiator instantiator = new GameObjectInstantiator(
                import,
                parent,
                settings: new InstantiationSettings { SceneObjectCreation = SceneObjectCreation.Always }
            );

            bool instantiated = await import.InstantiateMainSceneAsync(instantiator);

            if (!instantiated || instantiator.SceneTransform == null)
            {
                Debug.LogError($"ModelCache: no se pudo instanciar la escena de {relativePath}");
                return null;
            }

            // La misma pregunta que antes de instanciar, repetida despues del segundo await.
            // Un padre puede morir durante cualquiera de las dos esperas, y comprobar solo
            // la primera dejaba el caso en que el .glb ya estaba en cache —la espera larga
            // es la segunda—: el modelo acababa suelto en la raiz de la escena, sin entidad
            // que lo destruya nunca. Lo que ya se ha instanciado se destruye aqui porque
            // quien lo pidio no va a recibirlo.
            if (expectsParent && parent == null)
            {
                Debug.Log($"ModelCache: se descarta {relativePath}, quien lo pidio murio mientras se instanciaba");
                UnityEngine.Object.Destroy(instantiator.SceneTransform.gameObject);
                return null;
            }

            GameObject root = instantiator.SceneTransform.gameObject;

            // El nombre que trae el .glb suele ser el que quedo en Blender y no dice nada
            // util al mirar la jerarquia en ejecucion. El del fichero identifica al item.
            root.name = Path.GetFileNameWithoutExtension(relativePath);

            return root;
        }

        /// <summary>
        /// Devuelve la carga en curso o la arranca si es la primera vez que se pide.
        /// </summary>
        private Task<GltfImport> Get(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return Task.FromResult<GltfImport>(null);

            if (_imports.TryGetValue(relativePath, out Task<GltfImport> existing)) return existing;

            Task<GltfImport> loading = Load(relativePath);
            _imports[relativePath] = loading;
            return loading;
        }

        /// <summary>
        /// Lee el fichero y lo interpreta.
        ///
        /// Se leen los bytes a mano en vez de darle la ruta a glTFast para que la resuelva
        /// el: asi el camino es el mismo que el de las texturas, y no dependemos de como
        /// cada plataforma entienda una URI <c>file://</c> — en Android, por ejemplo, la
        /// carpeta de datos vive dentro del propio paquete y no es un fichero normal.
        ///
        /// La Uri se sigue pasando porque un glTF puede referenciar recursos externos y sin
        /// ella no sabria desde donde buscarlos. Con los .glb que exportamos no ocurre, ya
        /// que llevan todo dentro, pero eso es una propiedad de nuestros ficheros y no del
        /// formato.
        ///
        /// Se usa <c>Load</c> y no <c>LoadGltfBinary</c>: el segundo hace lo mismo pero esta
        /// marcado obsoleto desde la version 6 del paquete.
        /// </summary>
        /// <returns>El modelo interpretado, o null si falta el fichero o esta corrupto</returns>
        private async Task<GltfImport> Load(string relativePath)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"ModelCache: model not found at {fullPath}");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(fullPath);

            GltfImport import = new GltfImport();
            bool loaded = await import.Load(bytes, new Uri(fullPath));

            if (!loaded)
            {
                Debug.LogError($"ModelCache: {relativePath} no es un glTF binario valido");
                import.Dispose();
                return null;
            }

            return import;
        }

        /// <summary>
        /// Vacia la cache y libera las mallas y texturas cargadas.
        ///
        /// Los fallos tambien se quedan cacheados: si un modelo falta, la entrada nula
        /// permanece para no releer el mismo hueco una vez por entidad y por fotograma. Eso
        /// significa que reponer el fichero con el juego corriendo no basta, y este metodo
        /// es la forma de volver a intentarlo — util al recargar el catalogo, no en juego.
        ///
        /// No destruye los GameObjects ya instanciados: son del que los pidio.
        /// </summary>
        public void Clear()
        {
            foreach (Task<GltfImport> task in _imports.Values)
            {
                if (task.IsCompletedSuccessfully) task.Result?.Dispose();
            }

            _imports.Clear();
        }
    }
}
