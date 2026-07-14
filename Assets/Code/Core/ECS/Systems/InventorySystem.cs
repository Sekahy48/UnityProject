using System;
using Core;
using ECS.Component;
using ECS.Entity;
using Events;
using Observer;

namespace ECS.Systems
{
    public class InventorySystem : IEventObserver
    {
        public void ProcessEntity(IEntity entity)
        {
            InventoryComponent inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent != null)
            {
                float totalVolume = inventoryComponent.Inventory.GetTotalVolume();
                float totalWeight = inventoryComponent.Inventory.GetTotalWeight();
                CoreLogger.Instance.Log("Total volume: " + totalVolume + ", Total weight: " + totalWeight);

                // Physical carry capacity check
                if (entity.HasComponent(typeof(BodyComponent)))
                {
                    float carryWeight = CarryCapacity.GetMaxCarryWeight(entity);
                    float carryVolume = CarryCapacity.GetMaxCarryVolume(entity);

                    if (carryVolume < totalVolume)
                    {
                        CoreLogger.Instance.LogWarning("Volume exceeds carry capacity. Max: " + carryVolume);
                    }

                    float weightRatio = totalWeight / carryWeight;
                    if (weightRatio > 0.6f && weightRatio <= 0.8f)
                    {
                        CoreLogger.Instance.Log("Heavy load. Speed reduced.");
                    }
                    else if (weightRatio > 0.8f && weightRatio < 1f)
                    {
                        CoreLogger.Instance.LogWarning("Overloaded. Speed heavily reduced, energy penalty.");
                    }
                    else if (weightRatio >= 1f)
                    {
                        CoreLogger.Instance.LogWarning("Cannot move due to excess weight.");
                    }

                }
                else if (entity.HasComponent(typeof(StorageComponent)))
                {
                    // TODO: StorageComponent capacity checks
                }
            }
        }
    
        public void UpdateOnEvent(GameEvent gameEvent)
        {
            if (gameEvent.GetEventType() == GameEventType.INVENTORY_CHANGED)
            {
                ProcessEntity(gameEvent.GetEntity());
            }
        }
    }
}