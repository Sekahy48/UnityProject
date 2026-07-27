using System;
using Core;
using ECS.Component;
using ECS.Entity;
using Events;
using Inventory;
using Observer;
using AC = Utils.ArgumentChecker;

namespace ECS.Systems
{
    public class InventorySystem : IEventObserver
    {
        private float EXTRA_WEIGHT = 0.70f; 
        private float OVERWEIGHT = 0.85f;
        private float IMMOBILE = 1; 

        /// <summary>
        /// Returns how many items can be added by weight, and outputs the InventoryComponent.
        /// Returns 0 if entity has no inventory.
        /// </summary>
        private int GetFitByWeight(IEntity entity, ItemEntity item, int amount, out InventoryComponent invComp)
        {
            invComp = entity.GetComponent<InventoryComponent>();
            if (invComp == null) return 0;

            float currentWeight = invComp.Inventory.GetTotalWeight();
            float itemWeight = item.GetComponent<BaseItemComponent>().GetWeight();
            float maxWeight = GetMaxWeight(entity);

            int fitByWeight = itemWeight > 0 ? (int)((maxWeight - currentWeight) / itemWeight) : amount;
            return Math.Min(amount, fitByWeight);
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
        public int TryAddItemAt(IEntity entity, ItemEntity item, int amount, int row, int col)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.AddItemAt(item, toAdd, row, col);
            EvaluateAndFireEvents(entity, remaining > 0);
            return remaining + (amount - toAdd);
        } 

        public void EvaluateAndFireEvents(IEntity entity, bool fullGrid)
        { 

            AC.CheckNotNull(entity, entity.GetName());
            InventoryComponent inventoryComponent = entity.GetComponent<InventoryComponent>();
            MovementComponent movementComponent = entity.GetComponent<MovementComponent>();
            AC.CheckNotNull(inventoryComponent, "inventoryComponent"); 
  
            float totalWeight = inventoryComponent.Inventory.GetTotalWeight();
            CoreLogger.Instance.Log("Total weight: " + totalWeight);

            // Physical carry capacity check
            if (entity.HasComponent(typeof(BodyComponent)) && movementComponent != null)
            {
                float carryWeight = GetMaxWeight(entity); 

                float weightRatio = totalWeight / carryWeight;
                if (weightRatio > EXTRA_WEIGHT && weightRatio <= OVERWEIGHT)
                {
                    EventBus.GetInstance().Post(new GameEvent(GameEventType.ExtraWeight , entity, movementComponent));
                    CoreLogger.Instance.Log("Heavy load. Speed reduced."); 
                }
                else if (weightRatio > OVERWEIGHT && weightRatio < IMMOBILE)
                {
                    EventBus.GetInstance().Post(new GameEvent(GameEventType.Overweight , entity, movementComponent));
                    CoreLogger.Instance.LogWarning("Overloaded. Speed heavily reduced, energy penalty.");
                }
                else if (weightRatio >= IMMOBILE)
                {
                    EventBus.GetInstance().Post(new GameEvent(GameEventType.Immobile , entity, movementComponent));
                    CoreLogger.Instance.LogWarning("Cannot move due to excess weight.");
                } 

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

        private float GetMaxWeight(IEntity entity)
        {
            if (entity.HasComponent(typeof(BodyComponent)))
                return CarryCapacity.GetMaxCarryWeight(entity);

            StorageComponent storage = entity.GetComponent<StorageComponent>();
            return storage != null ? storage.MaxWeight : float.MaxValue;
        }

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