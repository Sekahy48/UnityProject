using System;
using ECS.Component;
using ECS.Entity;
using Events;
using Observer;
using UnityEngine.Rendering;

namespace ECS.Systems
{
    public class InventorySystem : IEventObserver
    {
        public void ProcessEntity(IEntity entity)
        {
            InventoryComponent inventoryComponent = entity.GetComponent<InventoryComponent>(typeof(InventoryComponent));
            if (inventoryComponent != null)
            {
                float totalVolume = inventoryComponent.getInventory().GetTotalVolume(); 
                float totalWeight = inventoryComponent.getInventory().GetTotalWeight(); 
                UnityEngine.Debug.Log("Total volume: " + totalVolume +  ", Total weight: " + totalWeight );

                // --- Cálculo de capacidad física del personaje ---
                if (entity.HasComponent(typeof(FisiologicComponent)))
                {
                    float carryWeight = entity.GetComponent<FisiologicComponent>(typeof(FisiologicComponent)).GetMaxCarryWeight();
                    float carryVolume = entity.GetComponent<FisiologicComponent>(typeof(FisiologicComponent)).GetMaxCarryVolume();

                    if (carryVolume < totalVolume)
                    {
                        UnityEngine.Debug.Log("El personaje no puede llevar tanto volumen. Capacidad máxima: " + carryVolume);
                    }

                    float weightRatio = totalWeight / carryWeight;
                    if ( weightRatio > 0.6f && weightRatio <= 0.8f)
                    {
                        UnityEngine.Debug.Log("El personaje está cargando un peso considerable. Velocidad reducida.");
                    }
                    else if (weightRatio > 0.8f && weightRatio < 1f)
                    {
                        UnityEngine.Debug.Log("El personaje está sobrecargado. Velocidad reducida considerablemente y sufre penalización en la energia.");
                    } else if (weightRatio >= 1f)
                    {
                        UnityEngine.Debug.Log("El personaje no puede moverse debido al exceso de peso.");
                    }

                } else if (entity.HasComponent(typeof(StorageComponent)))
                {
                    // Implementar llegado el momento, tb StorageComponent. 
                }
            }
        }
    
        public void UpdateOnEvent(GameEvent gameEvent)
        {
            if (gameEvent.GetEventType() == GameEventType.INVETORY_CHANGED)
            {
                ProcessEntity(gameEvent.GetEntity());
            }
        }
    }
}