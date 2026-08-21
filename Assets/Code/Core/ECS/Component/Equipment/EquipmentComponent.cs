using System;
using System.Collections.Generic;
using ECS.Component.ItemComponents;
using ECS.Entity;
using AC = Utils.ArgumentChecker;
namespace ECS.Component.Equipment
{
    public class EquipmentComponent : IComponent
    {
        private List<EquipmentSlotType> _allowedSlots;
        private Dictionary<EquipmentSlotType, EquipmentSlot> _equipmentSlots;

        public EquipmentComponent(List<EquipmentSlotType> allowedSlots)
        {
            this._allowedSlots = allowedSlots;
            _equipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlot>();
        }

        public EquipmentSlot GetEquipmentSlot(EquipmentSlotType type)
        {
            return _equipmentSlots[type];
        }
        
        public bool AddSlot(EquipmentSlotType slotType, int maxLayers)
        {   
            AC.CheckNotNull(slotType, nameof(slotType));
            AC.CheckPositive(maxLayers, nameof(maxLayers));

            bool added = false; 

            if (_allowedSlots.Contains(slotType) && !_equipmentSlots.ContainsKey(slotType))
            {
                _equipmentSlots[slotType] = new EquipmentSlot(slotType, maxLayers);
                added = true; 
            }
            return added;
        }

        public EquipResult EquipItem(EquipmentSlotType slotType, ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));
            AC.CheckNotNull(slotType, nameof(slotType));

            EquipResult equiped;

            if (!_allowedSlots.Contains(slotType) || !_equipmentSlots.ContainsKey(slotType))
            {
                equiped = EquipResult.NoSlotFits; 
            }
            else 
            {
                equiped = _equipmentSlots[slotType].EquipItem(item);
            }
             

            return equiped;
        }

        public void UnequipItem(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item)); 
            
            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) throw new InvalidOperationException("Cannot unequip an item with no WearableComponent.");
            EquipmentSlot slot = _equipmentSlots[wearableComponent.GetTargetSlot()];
            if (!slot.UnequipItem(item)) throw new InvalidOperationException("Cannot unequip an item that is not equiped.");
            
        }

        public IComponent Clone()
        {
            EquipmentComponent equipmentComponent = new EquipmentComponent(new List<EquipmentSlotType>(_allowedSlots));
            foreach (KeyValuePair<EquipmentSlotType, EquipmentSlot> kvp in _equipmentSlots)
            {
                equipmentComponent.AddSlot(kvp.Key, kvp.Value.GetMaxLayers());
                List<ItemEntity> clonedItems = new List<ItemEntity>();
                foreach (ItemEntity item in kvp.Value.GetItems())
                {
                    clonedItems.Add((ItemEntity)item.Clone());
                }
                equipmentComponent._equipmentSlots[kvp.Key].SetItems(clonedItems);
            }
            return equipmentComponent;
        }

        public bool Equivalent(IComponent other)
        {
            if (other is EquipmentComponent otherEquipment)
            {
                if (_allowedSlots.Count != otherEquipment._allowedSlots.Count)
                    return false;

                foreach (EquipmentSlotType slotType in _allowedSlots)
                {
                    if (!otherEquipment._allowedSlots.Contains(slotType))
                        return false;

                    if (_equipmentSlots.ContainsKey(slotType) != otherEquipment._equipmentSlots.ContainsKey(slotType))
                        return false;

                    if (_equipmentSlots.ContainsKey(slotType)) {
                        EquipmentSlot thisSlot = _equipmentSlots[slotType];
                        EquipmentSlot otherSlot = otherEquipment._equipmentSlots[slotType];

                        if (thisSlot.GetEquippedItemCount() != otherSlot.GetEquippedItemCount() ||
                            thisSlot.GetMaxLayers() != otherSlot.GetMaxLayers())
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}