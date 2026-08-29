using System;
using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Entity;
using Events;
using Inventory;
using Observer;
using AC = Utils.ArgumentChecker;

namespace ECS.Systems
{
    public class InventorySystem : IReactiveSystem
    {
        private static readonly GameEventType[] _subscribedEvents =
        {
            GameEventType.InventoryChanged 
        };

        public IEnumerable<GameEventType> SubscribedEvents => _subscribedEvents;


        /// <summary>
        /// Returns how many items can be added by weight, and outputs the InventoryComponent.
        /// Returns 0 if entity has no inventory.
        /// </summary>
        private int GetFitByWeight(IEntity entity, ItemEntity item, int amount, out InventoryComponent invComp)
        {
            invComp = entity.GetComponent<InventoryComponent>();
            if (invComp == null) return 0;

            // La regla vive en CarryCapacity: aqui solo se resuelve el componente, que el
            // llamante reutiliza. Asi el veredicto de la UI consulta lo mismo que este camino.
            return CarryCapacity.FitByWeight(entity, invComp.Inventory, item, amount);
        }

        /// <summary>
        /// Skips the weight check, still resolving the inventory. Returns 0 if the entity has
        /// no inventory, same as GetFitByWeight.
        /// </summary>
        private int WholeAmount(IEntity entity, int amount, out InventoryComponent invComp)
        {
            invComp = entity.GetComponent<InventoryComponent>();
            return invComp == null ? 0 : amount;
        }

        /// <summary>
        /// Tries to stack items checking weight first, then grid.
        /// Stacks onto existing compatible node if possible, creates new nodes for overflow.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryStackOntoHere(IEntity entity, ItemEntity item, int amount)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.StackOntoHere(item, toAdd);
            EvaluateAndFireEvents(entity, remaining > 0);   
            return remaining + (amount - toAdd);
        }

        /// <summary>
        /// Tries to stack items onto a specific node by nodeId, checking weight first.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryStackOntoNode(IEntity entity, ItemEntity item, int amount, int nodeId)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.StackOntoNode(nodeId, item, toAdd);
            EvaluateAndFireEvents(entity, false);
            return remaining + (amount - toAdd);
        }

        /// <summary>
        /// Tries to add items at a specific grid position, checking weight first.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryAddItemAt(IEntity entity, ItemEntity item, int amount, int row, int col, int ignoreNodeId = -1)
        {
            // A node moving onto cells it already owns can only happen inside one inventory,
            // and reordering an inventory cannot change its total weight: the units are
            // already carried. Checking would also risk clamping the move for someone who is
            // already overweight, leaving the source node alive over cells the new node took.
            bool sameInventoryMove = ignoreNodeId != -1;

            InventoryComponent invComp;
            int toAdd = sameInventoryMove
                ? WholeAmount(entity, amount, out invComp)
                : GetFitByWeight(entity, item, amount, out invComp);

            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.AddItemAt(item, toAdd, row, col, ignoreNodeId);
            EvaluateAndFireEvents(entity, remaining > 0);
            return remaining + (amount - toAdd);
        }  

        public void EvaluateAndFireEvents(IEntity entity, bool fullGrid)
        {

            AC.CheckNotNull(entity, nameof(entity));
            InventoryComponent inventoryComponent = entity.GetComponent<InventoryComponent>();
            MovementComponent movementComponent = entity.GetComponent<MovementComponent>();
            AC.CheckNotNull(inventoryComponent, "inventoryComponent"); 
  
            float totalWeight = inventoryComponent.Inventory.GetTotalWeight();
            CoreLogger.Instance.Log("Total weight: " + totalWeight);

            // Physical carry capacity check
            if (entity.HasComponent(typeof(BodyComponent)) && movementComponent != null)
            {
                float carryWeight = GetMaxWeight(entity); 

                float weightRatio = carryWeight > 0 ? totalWeight / carryWeight : 1;

                GameEventType load = CarryCapacity.ClassifyLoad(weightRatio);
                EventBus.GetInstance().Post(new GameEvent(load, entity, movementComponent));
                LogLoad(load);

            }
            else if (entity.HasComponent(typeof(StorageComponent)))
            {
                // TODO: StorageComponent capacity checks
            }  

            if (fullGrid)
            {
                EventBus.GetInstance().Post(new GameEvent(GameEventType.InventoryFull , entity, inventoryComponent));
                CoreLogger.Instance.LogWarning("Inventory overflow: cannot transfere more object due to insufficient grid space.");
            }
        }

        /// <summary>
        /// Reports the encumbrance band. Separate from the classification itself:
        /// logging is a consequence of the band, not part of deciding it.
        /// </summary>
        private void LogLoad(GameEventType load)
        {
            switch (load)
            {
                case GameEventType.ExtraWeight:
                    CoreLogger.Instance.Log("Heavy load. Speed reduced.");
                    break;
                case GameEventType.Overweight:
                    CoreLogger.Instance.LogWarning("Overloaded. Speed heavily reduced, energy penalty.");
                    break;
                case GameEventType.Immobile:
                    CoreLogger.Instance.LogWarning("Cannot move due to excess weight.");
                    break;
                default:
                    CoreLogger.Instance.Log("No movement restrictions.");
                    break;
            }
        }

        private float GetMaxWeight(IEntity entity) => CarryCapacity.GetMaxLoad(entity);

        public void ProcessEntity(IEntity entity)
        {
            EvaluateAndFireEvents(entity, false);
        } 
    
        public void UpdateOnEvent(GameEvent gameEvent)
        {
            if (gameEvent.GetEventType() == GameEventType.InventoryChanged)
            {
                ProcessEntity(gameEvent.GetEntity());
            }
        }
    }
}