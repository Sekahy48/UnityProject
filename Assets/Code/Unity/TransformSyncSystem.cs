using ECS.Component;
using ECS.Entity;
using ECS.Systems;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Engine system (runs every frame with real deltaTime).
    /// Syncs PositionComponent (Core) with Transform (Unity).
    ///
    /// Sync direction:
    /// - If PositionComponent is dirty (Core modified it) → writes to Transform.
    /// - If not dirty → reads Transform and updates PositionComponent
    ///   (to reflect movements done by cameras, physics, animation, etc.)
    /// </summary>
    public class TransformSyncSystem : IGameSystem
    {
        public void Process(float deltaTime, EntityManager entityManager)
        {
            var entities = entityManager.GetEntitiesWithComponent(typeof(PositionComponent));

            foreach (var entity in entities)
            {
                if (!entity.HasComponent(typeof(UnityEntityComponent)))
                    continue;

                var pos = entity.GetComponent<PositionComponent>();
                var unity = entity.GetComponent<UnityEntityComponent>();
                Transform t = unity.GetGameObject().transform;

                if (pos.IsDirty())
                {
                    // Core modified the position → apply to Transform
                    t.position = new Vector3(pos.GetX(), pos.GetY(), pos.GetZ());
                    t.rotation = new Quaternion(pos.GetRotX(), pos.GetRotY(), pos.GetRotZ(), pos.GetRotW());
                    pos.ClearDirty();
                }
                else
                {
                    // Unity moved the object (cameras, physics) → read into the Core component
                    pos.SetPosition(t.position.x, t.position.y, t.position.z);
                    pos.SetRotation(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
                    pos.ClearDirty(); // SetPosition marks dirty, but here we don't want to re-sync
                }
            }
        }
    }
}
