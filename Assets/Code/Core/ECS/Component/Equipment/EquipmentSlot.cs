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
        /// <param name="ignored">Prenda que ya esta aqui pero se considera de paso, porque
        /// alguien la lleva en la mano y va a colocarla ahora. Sin esto, devolver al slot lo
        /// que acabas de sacar de el chocaria contra si mismo: la mano es una referencia y la
        /// prenda no sale de verdad hasta que se coloca. Es el mismo papel que juega
        /// ignoreNodeId en la rejilla — que ocupa sitio de verdad y que esta solo de paso.</param>
        public EquipResult CanEquip(ItemEntity item, ItemEntity ignored = null)
        {
            AC.CheckNotNull(item, nameof(item));

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) return EquipResult.NotWearable;
            if (!_enabled) return EquipResult.SlotDisabled;
            if (OccupiedLayers(ignored) >= _maxLayers) return EquipResult.MaxLayersReached;
            if (!wearableComponent.TargetSlots.Contains(SlotType)) return EquipResult.WrongSlot;
            if (ContainsGarmentCategory(wearableComponent.GarmentCategory, ignored)) return EquipResult.DuplicateCategory;

            // Con la capa exterior puesta solo caben prendas interiores, que se cuelan debajo.
            // Si la que bloquea es justo la que esta de paso, no bloquea nada.
            if (wearableComponent.IsTopLayer && IsTopLockedBySomeoneElse(ignored))
                return EquipResult.TopLayerBlocked;

            return EquipResult.SuccessEquip;
        }

        /// <summary>Capas que ocupan sitio de verdad: la que esta de paso no cuenta.</summary>
        private int OccupiedLayers(ItemEntity ignored)
        {
            int count = _equippedItems.Count;

            return ignored != null && _equippedItems.Contains(ignored) ? count - 1 : count;
        }

        /// <summary>
        /// Si la capa exterior sigue bloqueando una vez descontada la prenda de paso. Solo
        /// puede bloquear la ultima, que es la que ocupa el exterior.
        /// </summary>
        private bool IsTopLockedBySomeoneElse(ItemEntity ignored)
        {
            if (!_isTopLocked) return false;
            if (ignored == null || _equippedItems.Count == 0) return true;

            return !ReferenceEquals(_equippedItems[_equippedItems.Count - 1], ignored);
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

        /// <summary>
        /// Reemplaza el contenido del slot.
        /// </summary>
        /// <remarks>
        /// Recalcula <c>_isTopLocked</c> a partir de lo que entra, porque ese flag es una
        /// consecuencia de lo que hay puesto y no un estado independiente. Sin esto, un slot
        /// rellenado por aqui —que es como lo hace <c>EquipmentComponent.Clone</c>, y el jugador
        /// en partida es un clon del prototipo— se quedaba con el flag en false llevando una
        /// prenda exterior: <c>EquipItem</c> tomaba entonces la rama de anadir al final, asi que
        /// cualquier prenda equipada despues se colocaba POR ENCIMA de la coraza en vez de
        /// debajo, y el slot la pintaba a ella.
        /// </remarks>
        public void SetItems(List<ItemEntity> items)
        {
            AC.CheckNotNull(items, nameof(items));
            if (items.Count > _maxLayers) return;

            _equippedItems = items;
            _isTopLocked = HasTopLayerGarment();
        }

        /// <summary>
        /// Si alguna de las prendas puestas es de capa exterior.
        ///
        /// Se mira en todas y no solo en la ultima porque SetItems no garantiza orden: quien
        /// rellena el slot puede darlas en cualquiera. En el uso normal la exterior es la
        /// ultima, y entonces las dos formas de preguntarlo coinciden.
        /// </summary>
        private bool HasTopLayerGarment()
        {
            foreach (ItemEntity item in _equippedItems)
            {
                WearableComponent wearable = item.GetComponent<WearableComponent>();
                if (wearable != null && wearable.IsTopLayer) return true;
            }

            return false;
        }
        
        public List<ItemEntity> Items => _equippedItems;
        
        public ItemEntity GetTopItem()
        {
            return _equippedItems.Count == 0 ? null : _equippedItems[_equippedItems.Count - 1];
        }

        public ItemEntity GetItem(int layer)
        {
            return _equippedItems[layer];
        }
        
        /// <param name="ignored">Prenda de paso: no cuenta como ocupante de su categoria.</param>
        public bool ContainsGarmentCategory(GarmentCategory category, ItemEntity ignored = null)
        {
            foreach (ItemEntity item in _equippedItems)
            {
                if (ReferenceEquals(item, ignored)) continue;

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