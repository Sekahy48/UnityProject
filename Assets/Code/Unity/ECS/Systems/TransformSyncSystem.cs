using Core.ECS.Component; 
using Core.ECS.Systems; 
using Unity.ECS.Component;
using UnityEngine;
using Core.ECS;

namespace Unity.ECS.Systems
{
    /// <summary>
    /// Sistema del motor (se ejecuta cada fotograma con el deltaTime real).
    /// Sincroniza PositionComponent (Core) con Transform (Unity).
    ///
    /// Sentido de la sincronizacion:
    /// - Si PositionComponent esta sucio (lo cambio el Core): se escribe en el Transform.
    /// - Si no: se lee el Transform y se actualiza PositionComponent
    ///   (para reflejar lo que mueven las camaras, la fisica, la animacion, etc.)
    /// </summary>
    public class TransformSyncSystem : IPeriodicSystem
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

                if (pos.IsDirty)
                {
                    // El Core cambio la posicion: es un teletransporte. Con Rigidbody se mueve el
                    // cuerpo (si no, la fisica no se entera bien) y se para: lo teletransportado
                    // no sigue cayendo.
                    Vector3 position = new Vector3(pos.X, pos.Y, pos.Z);
                    Quaternion rotation = new Quaternion(pos.RotX, pos.RotY, pos.RotZ, pos.RotW);
                    if (t.TryGetComponent(out Rigidbody body))
                    {
                        body.position = position;
                        body.rotation = rotation;
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                    }
                    t.SetPositionAndRotation(position, rotation);   // que se vea en este fotograma
                    pos.ClearDirty();
                }
                else
                {
                    // Unity movio el objeto (camaras, fisica): se lee en el componente del Core
                    pos.SetPosition(t.position.x, t.position.y, t.position.z);
                    pos.SetRotation(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
                    pos.ClearDirty(); // SetPosition lo marca sucio, pero aqui no se quiere volver a sincronizar
                }
            }
        }
    }
}
