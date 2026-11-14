using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Unity.ECS.Component;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Da cuerpo fisico a lo que aparece en el mundo (Fase 1.5, M2; decisiones 40-43).
    ///
    /// <para>Dos momentos, porque el modelo 3D llega tarde: al enlazar, la entidad cae ya con
    /// una esfera provisional del tamano de su volumen de interaccion; cuando el modelo termina
    /// de cargar, la esfera se cambia por un collider convexo por cada malla, todos bajo el
    /// mismo Rigidbody (un solo solido: una tapa no se abre).</para>
    ///
    /// <para>Tambien es el unico sitio que traduce peso en masa (<see cref="ApplyMass"/>): lo
    /// usan el enlazado y MassSyncSystem, para que el minimo viva en un sitio.</para>
    ///
    /// <para>No vive en <c>Unity.Physics</c> a proposito: es el nombre de un paquete oficial de
    /// Unity (DOTS) y chocaria si algun dia se instala.</para>
    /// </summary>
    public static class PhysicsBodyBuilder
    {
        // La fisica no admite masa 0; el Core devuelve 0 cuando no sabe el peso
        public const float MIN_MASS = 0.01f;

        // Impulso al soltar (decision 45): la mano da siempre la misma cantidad de movimiento, asi que
        // lo ligero sale rapido y lo pesado casi cae a los pies. Con 4 kg*m/s y tope de 2 m/s:
        // manzana (0,2 kg) 2 m/s, espada (3,5 kg) 1,1 m/s, arcon vacio (8 kg) 0,5 m/s, arcon con
        // 50 kg dentro 0,07 m/s. Valores de partida para ajustar jugando.
        private const float TOSS_IMPULSE = 4f;
        private const float MAX_TOSS_SPEED = 2f;

        /// <summary>Masa del cuerpo = peso del Core, con el minimo que exige el motor.</summary>
        public static void ApplyMass(Rigidbody body, float kilograms)
            => body.mass = Mathf.Max(MIN_MASS, kilograms);

        /// <summary>
        /// Rigidbody y esfera provisional. Solo para lo que se crea en el mundo: el jugador trae
        /// su Rigidbody de la escena. La masa no la pone aqui: la pone el enlazado para todo lo
        /// que tenga cuerpo (UnityEntityLinker.SyncInitialMass). La velocidad, <see cref="Release"/>.
        /// </summary>
        public static Rigidbody AddBody(IEntity entity, GameObject host)
        {
            Rigidbody body = host.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            // Continuous y no Discrete: una manzana (unos 8 cm) cayendo desde la mano recorre su
            // propio tamano en cada paso de fisica y atravesaba el suelo, que es un plano sin
            // grosor. Continuous comprueba la trayectoria contra lo estatico (suelo, paredes).
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            AddProvisionalSphere(entity, host);

            return body;
        }

        /// <summary>
        /// Suelta lo recien creado desde el centro de quien lo suelta (decision 45): un impulso
        /// horizontal hacia donde mira su cuerpo, segun la masa, mas la velocidad horizontal que
        /// ya llevaba (decision 42; la vertical no, para que soltar en pleno salto no lo lance
        /// hacia arriba). Mientras sale, no choca con el (<see cref="DropperGrace"/>). Necesita
        /// la masa ya puesta: se llama despues de sincronizarla.
        /// </summary>
        /// <param name="origin">Quien lo solto, o null: entonces cae sin mas.</param>
        public static void Release(GameObject host, IEntity origin)
        {
            GameObject dropper = GameObjectOf(origin);
            if (dropper == null || !host.TryGetComponent(out Rigidbody body)) return;

            Vector3 forward = Vector3.ProjectOnPlane(dropper.transform.forward, Vector3.up).normalized;
            float speed = Mathf.Min(MAX_TOSS_SPEED, TOSS_IMPULSE / body.mass);
            Vector3 inherited = dropper.TryGetComponent(out Rigidbody dropperBody)
                ? Vector3.ProjectOnPlane(dropperBody.linearVelocity, Vector3.up)
                : Vector3.zero;

            body.linearVelocity = forward * speed + inherited;
            host.AddComponent<DropperGrace>().Begin(dropper);
        }

        /// <summary>
        /// Cambia la esfera provisional por un collider convexo por cada malla del modelo.
        ///
        /// <para>Si ninguna malla sirve (sin malla, o no legible en ejecucion: el riesgo de
        /// glTFast de la decision 41) se queda la esfera: cae igual, solo que todo rueda.</para>
        /// </summary>
        public static void ReplaceWithHulls(GameObject host, GameObject model)
        {
            if (host == null || model == null) return;
            if (!host.TryGetComponent(out Rigidbody _)) return;   // el jugador u otro sin cuerpo propio

            int added = 0;
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null) continue;
                if (!mesh.isReadable)
                {
                    Debug.LogWarning($"PhysicsBodyBuilder: la malla '{mesh.name}' no es legible en ejecucion; se queda la esfera provisional.");
                    continue;
                }

                MeshCollider hull = filter.gameObject.AddComponent<MeshCollider>();
                hull.sharedMesh = mesh;
                hull.convex = true;   // un Rigidbody dinamico solo admite colliders de malla convexos
                added++;

                // Si aun esta saliendo de quien lo solto, el collider nuevo tampoco le choca.
                // TryGetComponent y no GetComponent()?.: en el editor GetComponent devuelve un
                // "null falso" que el ?. no detecta.
                if (host.TryGetComponent(out DropperGrace grace)) grace.IgnoreToo(hull);
            }

            if (added > 0 && host.TryGetComponent(out SphereCollider sphere))
                Object.Destroy(sphere);
        }

        /// <summary>
        /// Esfera con el centro y el tamano del volumen de interaccion que la entidad trae de
        /// su arquetipo (decision 41). Para un volumen que no es esfera se toma la distancia
        /// del centro al punto mas alto: basta para una forma provisional.
        /// </summary>
        private static void AddProvisionalSphere(IEntity entity, GameObject host)
        {
            SphereCollider sphere = host.AddComponent<SphereCollider>();

            InteractionVolume volume = entity.GetComponent<InteractionVolumeComponent>()?.Volume
                                       ?? new SphereVolume(0.25f);

            (float cx, float cy, float cz) = volume.CenterPoint();
            sphere.center = new Vector3(cx, cy, cz);
            sphere.radius = volume is SphereVolume round
                ? round.Radius
                : Mathf.Max(0.01f, volume.TopPoint().y - cy);
        }

        private static GameObject GameObjectOf(IEntity entity)
            => entity?.GetComponent<UnityEntityComponent>()?.GetGameObject();
    }
}
