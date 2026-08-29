using System;
using Core;
using ECS.Component.Equipment;
using ECS.Entity;
using Events;
using Observer;

namespace ECS.System
{
    public class EquipmentSystem
    {
        public EquipResult TryEquip(IEntity entity, ItemEntity item, EquipmentSlotType slot)
        {
            EquipmentComponent equipmentComponent = entity.GetComponent<EquipmentComponent>();
            if (equipmentComponent == null) throw new InvalidOperationException("Cannot equip items into an entity with no EquipmentComponent");

            EquipResult result = equipmentComponent.EquipItem(slot, item);
            CoreLogger.Instance.Log(result.GetMessage());

            if (result == EquipResult.Success) EventBus.GetInstance().Post(new GameEvent(GameEventType.EquipmentChanged, entity, equipmentComponent));

            return result; 
        }

        public EquipResult TryUnequip(IEntity entity, ItemEntity item)
        {
            EquipmentComponent equipmentComponent = entity.GetComponent<EquipmentComponent>();
            if (equipmentComponent == null) throw new InvalidOperationException("Cannot unequip items into an entity with no EquipmentComponent");
            
            equipmentComponent.UnequipItem(item);

            EventBus.GetInstance().Post(new GameEvent(GameEventType.EquipmentChanged, entity, equipmentComponent));
            CoreLogger.Instance.Log(EquipResult.Success.GetMessage());

            return EquipResult.Success;
        }
    }
}