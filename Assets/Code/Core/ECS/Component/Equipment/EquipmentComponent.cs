using System;
using System.Collections.Generic;
using ECS.Entity;
using Utils;

namespace ECS.Component
{
    public class EquipmentComponent : IComponent
    {
        private List<EquipmentSlotType> allowedSlots;
        private Dictionary<EquipmentSlotType, EquipmentSlot> equipmentSlots;

        public EquipmentComponent(List<EquipmentSlotType> allowedSlots)
        {
            this.allowedSlots = allowedSlots;
            equipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlot>();
        }

        public bool AddSlot(EquipmentSlotType slotType, int capacity)
        {   
            ArgumentChecker.CheckNotNull(slotType, nameof(slotType));
            ArgumentChecker.CheckPositive(capacity, nameof(capacity));

            bool added = false;

            if (!allowedSlots.Contains(slotType))
                return false;

            if (equipmentSlots.ContainsKey(slotType))
                return false;
            else
            {
                equipmentSlots[slotType] = new EquipmentSlot(slotType, capacity);
                added = true; 
            }
            return added;
        }

        public bool EquipItem(EquipmentSlotType slotType, ItemEntity item)
        {
            ArgumentChecker.CheckNotNull(item, nameof(item));
            ArgumentChecker.CheckNotNull(slotType, nameof(slotType));

            bool equiped = false;

            if (!allowedSlots.Contains(slotType))
                return false;

            if (!equipmentSlots.ContainsKey(slotType))
                return false;
            else {
                equiped = equipmentSlots[slotType].EquipItem(item);
            }
             

            return equiped;
        }

        public IComponent Clone()
        {
            EquipmentComponent equipmentComponent = new EquipmentComponent(new List<EquipmentSlotType>(allowedSlots));
            foreach (KeyValuePair<EquipmentSlotType, EquipmentSlot> kvp in equipmentSlots)
            {
                equipmentComponent.AddSlot(kvp.Key, kvp.Value.GetMaxAmount());
                List<ItemEntity> clonedItems = new List<ItemEntity>();
                foreach (ItemEntity item in kvp.Value.GetItems())
                {
                    clonedItems.Add((ItemEntity)item.Clone());
                }
                equipmentComponent.equipmentSlots[kvp.Key].SetItems(clonedItems);
            }
            return equipmentComponent;
        }

        public bool Equivalent(IComponent other)
        {
            if (other is EquipmentComponent otherEquipment)
            {
                if (allowedSlots.Count != otherEquipment.allowedSlots.Count)
                    return false;

                foreach (EquipmentSlotType slotType in allowedSlots)
                {
                    if (!otherEquipment.allowedSlots.Contains(slotType))
                        return false;

                    if (equipmentSlots.ContainsKey(slotType) != otherEquipment.equipmentSlots.ContainsKey(slotType))
                        return false;

                    if (equipmentSlots.ContainsKey(slotType)) {
                        EquipmentSlot thisSlot = equipmentSlots[slotType];
                        EquipmentSlot otherSlot = otherEquipment.equipmentSlots[slotType];

                        if (thisSlot.GetEquippedItemCount() != otherSlot.GetEquippedItemCount() ||
                            thisSlot.GetMaxAmount() != otherSlot.GetMaxAmount())
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}