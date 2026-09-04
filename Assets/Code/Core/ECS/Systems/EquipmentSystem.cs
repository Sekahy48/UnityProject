using System;
using System.Collections.Generic;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Core.Events; 

namespace Core.ECS.Systems
{
    public class EquipmentSystem : IReactiveSystem
    {
        private static readonly GameEventType[] _subscribedEvents =
        {
            /*Emtpy for the moment*/
        };

        public IEnumerable<GameEventType> SubscribedEvents => _subscribedEvents;

        public EquipResult TryEquip(IEntity entity, ItemEntity item, List<EquipmentSlotType> slots)
        {
            EquipmentComponent equipmentComponent = entity.GetComponent<EquipmentComponent>();
            if (equipmentComponent == null) throw new InvalidOperationException("Cannot equip items into an entity with no EquipmentComponent");
            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) 
                return EquipResult.NotWearable;

            EquipResult result = equipmentComponent.EquipItem(slots, item, wearableComponent.FullOcupancy);
            CoreLogger.Instance.Log(result.GetMessage());

            if (result == EquipResult.SuccessEquip) EventBus.GetInstance().Post(new GameEvent(GameEventType.EquipmentChanged, entity, equipmentComponent));

            return result; 
        }

        public EquipResult TryUnequip(IEntity entity, ItemEntity item, List<EquipmentSlotType> slotTypes, bool anounce = true)
        {
            EquipmentComponent equipmentComponent = entity.GetComponent<EquipmentComponent>();
            if (equipmentComponent == null) throw new InvalidOperationException("Cannot unequip items into an entity with no EquipmentComponent");
            
            foreach (EquipmentSlotType slotType in slotTypes)
                equipmentComponent.UnequipItem(item, slotType); 
            EventBus.GetInstance().Post(new GameEvent(GameEventType.EquipmentChanged, entity, equipmentComponent));
            CoreLogger.Instance.Log(EquipResult.SuccessUnequip.GetMessage());

            return EquipResult.SuccessUnequip;
        } 

        public void UpdateOnEvent(GameEvent gameEvent)
        { 
        }
    }
}