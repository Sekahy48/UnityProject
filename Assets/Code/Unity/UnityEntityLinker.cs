using Core;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.Item;
using Unity.ECS.Component;
using Unity.Services;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Links Core entities with Unity GameObjects.
    /// Adds UnityEntityComponent and syncs the initial position
    /// from the GameObject's Transform to the Core PositionComponent.
    /// </summary>
    public class UnityEntityLinker : IEntityLinker
    {
        public void Link(IEntity entity, EntityType entityType)
        {
            GameObject go = ResolveGameObject(entity, entityType, out bool created);
            if (go == null)
            {
                Debug.LogError($"UnityEntityLinker: No se pudo resolver GameObject para '{entityType}'.");
                return;
            }

            // Add the Unity bridge component (lives outside the Core ECS)
            entity.AddComponent(new UnityEntityComponent(go));

            SyncInitialPosition(entity, go, created);
            AttachModel(entity, go);
        }

        /// <summary>
        /// Da con el GameObject que representa a la entidad.
        ///
        /// Hay dos casos y son distintos de verdad, no una falta de generalidad. El jugador
        /// <b>se busca</b>: ya esta puesto en la escena con su prefab, su camara y sus
        /// controles, y no se construye desde el catalogo. Todo lo demas <b>se crea</b>: una
        /// manzana tirada no existe hasta que alguien la tira.
        ///
        /// Lo que se crea es un objeto vacio, no el modelo. El objeto vacio es la entidad de
        /// cara al motor —lleva la posicion, y llevara el collider y lo que se le cuelgue—,
        /// y el modelo es solo su aspecto, que vive dentro y se puede sustituir sin tocar lo
        /// demas. Si el modelo fuera el objeto de la entidad, cambiar de etapa obligaria a
        /// destruir y rehacer la entidad entera.
        /// </summary>
        /// <param name="created">Si el objeto acaba de nacer aqui. Lo necesita el
        /// sincronizado de posicion, que no puede adivinarlo mirando las coordenadas.</param>
        private GameObject ResolveGameObject(IEntity entity, EntityType entityType, out bool created)
        {
            if (entityType == EntityType.Player)
            {
                created = false;
                return GameObject.FindWithTag("MainPlayer");
            }

            created = true;
            return new GameObject($"{entityType}-{entity.GetIdAsInt()}");
        }

        /// <summary>
        /// Deja Core y el motor de acuerdo sobre donde esta la entidad.
        ///
        /// La direccion depende de quien mande, y eso lo dice quien resolvio el objeto, no
        /// las coordenadas. Para lo que ya estaba en la escena manda el Transform, porque el
        /// nivel se monta en el editor. Para lo que se acaba de crear manda Core, porque el
        /// objeto nacio en el origen y la posicion buena la puso quien decidio crearlo.
        ///
        /// Se aplica aqui en vez de dejarlo sucio para que lo recoja
        /// <see cref="TransformSyncSystem"/>, que lo haria bien pero un fotograma mas tarde:
        /// el tiempo justo para ver el monton aparecer en el origen del mundo y saltar a su
        /// sitio.
        /// </summary>
        private void SyncInitialPosition(IEntity entity, GameObject go, bool created)
        {
            PositionComponent pos = entity.GetComponent<PositionComponent>();
            if (pos == null) return;

            Transform t = go.transform;

            if (created)
            {
                t.position = new Vector3(pos.X, pos.Y, pos.Z);
                t.rotation = new Quaternion(pos.RotX, pos.RotY, pos.RotZ, pos.RotW);
            }
            else
            {
                pos.SetPosition(t.position.x, t.position.y, t.position.z);
                pos.SetRotation(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
            }

            pos.ClearDirty();
        }

        /// <summary>
        /// Cuelga el modelo 3D de la entidad cuando termine de cargar.
        ///
        /// <para><b>No se espera a proposito.</b> <c>Link</c> es sincrono y quien crea
        /// entidades no deberia bloquearse por un fichero: la entidad queda enlazada y
        /// colocada de inmediato, y el modelo aparece unos milisegundos despues. A partir de
        /// la segunda vez que se pide el mismo modelo es instantaneo, porque la cache ya lo
        /// tiene interpretado.</para>
        ///
        /// <para>El precio es una ventana en la que la entidad existe y no se ve. Si esa
        /// ventana llega a molestar, la salida no es esperar aqui sino precargar al arrancar
        /// los modelos del catalogo.</para>
        /// </summary>
        private async void AttachModel(IEntity entity, GameObject host)
        {
            string path = ItemModelResolver.ResolvePath(entity);
            if (string.IsNullOrEmpty(path)) return;

            GameObject model = await ModelCache.Instance.Instantiate(path, host.transform);
            if (model == null) return;

            MeasureInteractionVolume(entity, host, model);
        }

        /// <summary>
        /// Ajusta el volumen de interaccion de la entidad a lo que ocupa su modelo.
        ///
        /// <para>El volumen no se declara en el catalogo porque ya esta en la geometria:
        /// guardarlo aparte seria el mismo hecho escrito dos veces, y bastaria cambiar un
        /// modelo y olvidar el numero para que la entidad se pudiera alcanzar desde donde no
        /// toca. Aqui se mide y se acabo.</para>
        ///
        /// <para>Hasta este momento la entidad lleva el volumen por defecto de su arquetipo,
        /// asi que se le puede apuntar desde que aparece; esto solo lo afina.</para>
        ///
        /// <para>Las envolventes de los <c>Renderer</c> son cajas alineadas al mundo, no al
        /// objeto. Con una entidad girada la caja resultante sobra por las esquinas. Es
        /// asumible para decidir alcance —sobrar unos centimetros no cambia ninguna
        /// respuesta— y evita recorrer vertices para calcular la caja ajustada.</para>
        /// </summary>
        private void MeasureInteractionVolume(IEntity entity, GameObject host, GameObject model)
        {
            InteractionVolumeComponent volume = entity.GetComponent<InteractionVolumeComponent>();
            if (volume == null) return;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            Vector3 localCenter = host.transform.InverseTransformPoint(bounds.center);

            volume.SetVolume(new BoxVolume(
                localCenter.x, localCenter.y, localCenter.z,
                bounds.extents.x, bounds.extents.y, bounds.extents.z));
        }
    }
}
