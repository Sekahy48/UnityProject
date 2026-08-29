using System.Collections.Generic;
using ECS.Component.ItemComponents;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace ECS.Component.Equipment
{
    public class EquipmentSlot
    {
        private EquipmentSlotType _slotType;
        private List<ItemEntity> _equippedItems;
        private bool _enabled;
        private bool _isTopLocked;        private int _maxLayers;

        public EquipmentSlot(EquipmentSlotType type, int maxLayers)
        {
            AC.CheckNotNull(type, nameof(type));
            AC.CheckPositive(maxLayers, nameof(maxLayers));
            _slotType = type;
            _maxLayers = maxLayers;
            _equippedItems = new List<ItemEntity>();
            _enabled = true;
            _isTopLocked = false;
        }

        public void ClearSlot()
        {
            _equippedItems.Clear();
        }

        public int GetEquippedItemCount()
        {
            return _equippedItems.Count;
        }

        public int MaxLayers => _maxLayers;

        public EquipResult EquipItem(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) return EquipResult.NotWearable;
            if (!_enabled) return EquipResult.SlotDisabled;
            if (_equippedItems.Count >= _maxLayers) return EquipResult.MaxLayersReached;
            if (!_slotType.Equals(wearableComponent.TargetSlot)) return EquipResult.WrongSlot;
            if (ContainsGarmentCategory(wearableComponent.GarmentCategory)) return EquipResult.DuplicateCategory;

            if (!_isTopLocked)
            {
                _equippedItems.Add(item);
                _isTopLocked = wearableComponent.IsTopLayer;
            }
            else if (!wearableComponent.IsTopLayer)
            {
                _equippedItems.Insert(_equippedItems.Count - 1, item);
            }
            else
            {
                return EquipResult.TopLayerBlocked;
            }

            return EquipResult.Success;
        }

        public bool UnequipItem(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));
            return _equippedItems.Remove(item);
        }

        public void SetItems(List<ItemEntity> items)
        {
            AC.CheckNotNull(items, nameof(items));
            if (items.Count <= _maxLayers)
            {
                _equippedItems = items;
            }
        }
        
        public List<ItemEntity> Items => _equippedItems;
        
        public ItemEntity GetTopItem()
        {
            return _equippedItems[_equippedItems.Count - 1];
        }
        
        public bool ContainsGarmentCategory(GarmentCategory category)
        { 
            foreach (ItemEntity item in _equippedItems)
            {
                WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
                if (wearableComponent != null && wearableComponent.GarmentCategory.Equals(category))
                {
                    return true;    
                }
            }
            return false;
        }

        public bool IsTopLocked => _isTopLocked;

        //TODO Make getters and a criteria-based remover

        public void Enable() => _enabled = true;
        public void Disable() => _enabled = false;
        public bool IsEnabled => _enabled;
    }
}