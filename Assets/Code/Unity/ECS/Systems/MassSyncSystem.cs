using System.Collections.Generic;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.Inventory;
using Unity.ECS.Component;
using UnityEngine;
using AC = Core.Utils.ArgumentChecker;

namespace Unity.ECS.Systems
{
    /// <summary>
    /// Mantiene la masa de los Rigidbody igual al peso de su entidad en el Core (decision 31).
    /// Escucha WeightChanged, que avisa de que el peso PUEDE haber cambiado: poner la misma
    /// masa dos veces no hace dano. Las entidades sin cuerpo en el motor (una mochila puesta,
    /// una bolsa dentro del inventario) se ignoran.
    /// </summary>
    public class MassSyncSystem : IReactiveSystem
    { 

        public IEnumerable<GameEventType> SubscribedEvents { get; } = new[]
        {
            GameEventType.WeightChanged
        };

        public void UpdateOnEvent(GameEvent gameEvent)
        {
            AC.CheckNotNull(gameEvent, nameof(gameEvent));
            IEntity entity = gameEvent.GetEntity();
            AC.CheckNotNull(entity, "gameEvent.Entity");   // WeightChanged siempre va a una entidad

            switch (gameEvent.GetEventType())
            {
                case GameEventType.WeightChanged:
                    UnityEntityComponent unity = entity.GetComponent<UnityEntityComponent>();
                    if (unity != null && unity.GetGameObject().TryGetComponent(out Rigidbody body))
                        PhysicsBodyBuilder.ApplyMass(body, WeightCalculator.Of(entity));
                    break;
            }
        }
    }
}