using Core;
using ECS.Component;
using ECS.Entity;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Vincula entidades Core con GameObjects de Unity.
    /// Añade UnityEntityComponent y sincroniza la posición inicial
    /// desde el Transform del GameObject al PositionComponent Core.
    /// </summary>
    public class UnityEntityLinker : IEntityLinker
    {
        public void Link(IEntity entity, string entityType)
        {
            GameObject go = ResolveGameObject(entityType);
            if (go == null)
            {
                Debug.LogError($"UnityEntityLinker: No se pudo resolver GameObject para '{entityType}'.");
                return;
            }

            // Añadir el componente puente Unity (vive fuera del ECS Core)
            entity.AddComponent(new UnityEntityComponent(go));

            // Sincronizar posición inicial: Transform → PositionComponent
            var pos = entity.GetComponent<PositionComponent>();
            if (pos != null)
            {
                Transform t = go.transform;
                pos.SetPosition(t.position.x, t.position.y, t.position.z);
                pos.SetRotation(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
                pos.ClearDirty();
            }
        }

        /// <summary>
        /// Resuelve qué GameObject corresponde a un tipo de entidad.
        /// Para el jugador, busca por tag. Para otros tipos, se puede
        /// extender con Instantiate de prefabs, pool, etc.
        /// </summary>
        private GameObject ResolveGameObject(string entityType)
        {
            return entityType switch
            {
                "playerEntity" => GameObject.FindWithTag("MainPlayer"),
                _ => null
            };
        }
    }
}
