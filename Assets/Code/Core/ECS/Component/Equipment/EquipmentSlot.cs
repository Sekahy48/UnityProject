using System.Collections.Generic;
using ECS.Entity;
using Utils;

namespace ECS.Component
{
    public class EquipmentSlot
    {
        private EquipmentSlotType slotType;
        private List<ItemEntity> equippedItems;
        private int maxAmount;

        public EquipmentSlot(EquipmentSlotType type, int maxAmount)
        {
            ArgumentChecker.CheckNotNull(type, nameof(type));
            ArgumentChecker.CheckPositive(maxAmount, nameof(maxAmount));
            this.slotType = type;
            this.maxAmount = maxAmount;
            this.equippedItems = new List<ItemEntity>();
        }

        public bool EquipItem(ItemEntity item)
        {
            ArgumentChecker.CheckNotNull(item, nameof(item));
            bool equiped = false;
            if (equippedItems.Count < maxAmount)
            {
                equippedItems.Add(item);
                equiped = true;
            }
            return equiped;
        }

        public void ClearSlot()
        {
            equippedItems.Clear();
        }

        public int GetEquippedItemCount()
        {
            return equippedItems.Count;
        }

        public int GetMaxAmount()
        {
            return maxAmount;
        }

        public void SetItems(List<ItemEntity> items)
        {
            ArgumentChecker.CheckNotNull(items, nameof(items));
            if (items.Count <= maxAmount)
            {
                this.equippedItems = items;
            }
        }
        
        //TODO Make getters and a criteria-based remover
    }
}