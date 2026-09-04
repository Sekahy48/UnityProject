using System.Collections.Generic;
using System.Linq;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.ECS.Component.Equipment
{
    public class EquipmentSlot
    {
        public EquipmentSlotType SlotType {get; private set;}
        private List<ItemEntity> _equippedItems;
        private bool _enabled;
        private bool _isTopLocked;        
        private int _maxLayers;

        public EquipmentSlot(EquipmentSlotType type, int maxLayers)
        {
            AC.CheckNotNull(type, nameof(type));
            AC.CheckPositive(maxLayers, nameof(maxLayers));
            SlotType = type;
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

        /// <summary>
        /// Si esta prenda entraria aqui, y si no, por que. No toca nada.
        ///
        /// Existe para que la UI pueda pintar el veredicto antes de soltar sin arriesgarse a
        /// mentir: EquipItem empieza llamando a este metodo, asi que la respuesta y la
        /// operacion real no pueden discrepar. Duplicar las guardas en un metodo aparte seria
        /// el camino corto para que un dia el fantasma se pinte verde y el equipado falle.
        /// </summary>
        public EquipResult CanEquip(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) return EquipResult.NotWearable;
            if (!_enabled) return EquipResult.SlotDisabled;
            if (_equippedItems.Count >= _maxLayers) return EquipResult.MaxLayersReached;
            if (!wearableComponent.TargetSlots.Contains(SlotType)) return EquipResult.WrongSlot;
            if (ContainsGarmentCategory(wearableComponent.GarmentCategory)) return EquipResult.DuplicateCategory;

            // Con la capa exterior puesta solo caben prendas interiores, que se cuelan debajo.
            if (_isTopLocked && wearableComponent.IsTopLayer) return EquipResult.TopLayerBlocked;

            return EquipResult.SuccessEquip;
        }

        public EquipResult EquipItem(ItemEntity item)
        {
            EquipResult verdict = CanEquip(item);
            if (verdict != EquipResult.SuccessEquip) return verdict;

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();

            if (!_isTopLocked)
            {
                _equippedItems.Add(item);
                _isTopLocked = wearableComponent.IsTopLayer;
            }
            else
            {
                // Interior con exterior puesta: entra justo debajo de la superior.
                _equippedItems.Insert(_equippedItems.Count - 1, item);
            }

            return EquipResult.SuccessEquip;
        }

        public bool UnequipItem(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));
            bool removed = _equippedItems.Remove(item);
            if (removed && item.GetComponent<WearableComponent>().IsTopLayer)
                _isTopLocked = false;
            return removed;
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
            CoreLogger.Instance.Log(_equippedItems.Count.ToString());
            return _equippedItems.Count == 0 ? null : _equippedItems[_equippedItems.Count - 1];
        }

        public ItemEntity GetItem(int layer)
        {
            return _equippedItems[layer];
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