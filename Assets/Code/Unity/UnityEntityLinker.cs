using Core;
using ECS.Component;
using ECS.Entity;
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
        public void Link(IEntity entity, string entityType)
        {
            GameObject go = ResolveGameObject(entityType);
            if (go == null)
            {
                Debug.LogError($"UnityEntityLinker: No se pudo resolver GameObject para '{entityType}'.");
                return;
            }

            // Add the Unity bridge component (lives outside the Core ECS)
            entity.AddComponent(new UnityEntityComponent(go));

            // Sync initial position: Transform → PositionComponent
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
        /// Resolves which GameObject corresponds to an entity type.
        /// For the player, looks up by tag. For other types, this can be
        /// extended with prefab Instantiate, pooling, etc.
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
