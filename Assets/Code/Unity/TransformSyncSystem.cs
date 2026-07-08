using ECS.Component;
using ECS.Entity;
using ECS.Systems;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Engine system (corre cada frame con deltaTime real).
    /// Sincroniza PositionComponent (Core) con Transform (Unity).
    ///
    /// Dirección del sync:
    /// - Si PositionComponent está dirty (Core lo modificó) → escribe en Transform.
    /// - Si no está dirty → lee Transform y actualiza PositionComponent
    ///   (para reflejar movimientos hechos por cámaras, físicas, animación, etc.)
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
                    // Core modificó la posición → aplicar al Transform
                    t.position = new Vector3(pos.GetX(), pos.GetY(), pos.GetZ());
                    t.rotation = new Quaternion(pos.GetRotX(), pos.GetRotY(), pos.GetRotZ(), pos.GetRotW());
                    pos.ClearDirty();
                }
                else
                {
                    // Unity movió el objeto (cámaras, físicas) → leer al componente Core
                    pos.SetPosition(t.position.x, t.position.y, t.position.z);
                    pos.SetRotation(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
                    pos.ClearDirty(); // SetPosition marca dirty, pero aquí no queremos re-sync
                }
            }
        }
    }
}
