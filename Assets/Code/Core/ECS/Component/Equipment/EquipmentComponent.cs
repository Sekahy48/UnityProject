using System;
using System.Collections.Generic;
using System.Linq;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity; 
using AC = Core.Utils.ArgumentChecker;
namespace Core.ECS.Component.Equipment
{
    public class EquipmentComponent : IComponent
    {
        private List<EquipmentSlotType> _allowedSlots;
        public Dictionary<EquipmentSlotType, EquipmentSlot> EquipmentSlots {get; private set;}

        public EquipmentComponent(List<EquipmentSlotType> allowedSlots)
        {
            this._allowedSlots = allowedSlots;
            EquipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlot>();
        }

        public EquipmentSlot GetEquipmentSlot(EquipmentSlotType type)
        {
            return EquipmentSlots[type];
        }

        /// <summary>
        /// Las prendas puestas, cada una una sola vez.
        /// </summary>
        /// <remarks>
        /// Recorrer <see cref="EquipmentSlots"/> y sus capas no equivale a esto: una prenda de
        /// ocupacion completa —un arco a dos manos, una mochila que ocupa espalda y hombros—
        /// esta registrada en todos sus slots a la vez, asi que ese recorrido la visita una vez
        /// por slot. Para contar (peso, pestañas, inventarios equipados) hay que usar este
        /// metodo; el recorrido por slots solo sirve cuando lo que importa es el slot en si.
        /// </remarks>
        public IEnumerable<ItemEntity> EquippedItems()
        {
            HashSet<ItemEntity> seen = new HashSet<ItemEntity>();

            foreach (EquipmentSlot slot in EquipmentSlots.Values)
                foreach (ItemEntity item in slot.Items)
                    if (seen.Add(item)) yield return item;
        }
        
        public bool AddSlot(EquipmentSlotType slotType, int maxLayers)
        {   
            AC.CheckNotNull(slotType, nameof(slotType));
            AC.CheckPositive(maxLayers, nameof(maxLayers));

            bool added = false; 

            if (_allowedSlots.Contains(slotType) && !EquipmentSlots.ContainsKey(slotType))
            {
                EquipmentSlots[slotType] = new EquipmentSlot(slotType, maxLayers);
                added = true; 
            }
            return added;
        }

        public EquipResult EquipItem(EquipmentSlotType slotType, ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));
            AC.CheckNotNull(slotType, nameof(slotType));

            EquipResult equiped;

            if (!_allowedSlots.Contains(slotType) || !EquipmentSlots.ContainsKey(slotType))
            {
                equiped = EquipResult.NoSlotFits; 
            }
            else 
            {
                equiped = EquipmentSlots[slotType].EquipItem(item);
            }
             

            return equiped;
        }

        /// <summary>
        /// Si la prenda entraria, y si no, por que. No toca nada.
        ///
        /// Recorre las mismas decisiones que EquipItem y en el mismo orden, incluida la regla
        /// de todo-o-nada de la ocupacion completa: un arco a dos manos no "entra a medias",
        /// asi que basta con que un slot lo rechace para que el veredicto sea ese rechazo.
        /// </summary>
        /// <param name="ignored">Prenda que ya ocupa esos slots pero esta de paso, porque se
        /// lleva en la mano y va a colocarse ahora. Ver EquipmentSlot.CanEquip.</param>
        public EquipResult CanEquip(IReadOnlyList<EquipmentSlotType> slotTypes, ItemEntity item,
                                    bool fullOcupancy = false, ItemEntity ignored = null)
        {
            if (slotTypes.Count == 0)
                throw new InvalidOperationException("Cannot evaluate equipping an item while providing no possible slots.");

            AC.CheckNotNull(item, nameof(item));

            foreach (EquipmentSlotType slotType in slotTypes)
            {
                if (!_allowedSlots.Contains(slotType) || !EquipmentSlots.ContainsKey(slotType))
                    return EquipResult.NoSlotFits;

                EquipResult verdict = EquipmentSlots[slotType].CanEquip(item, ignored);

                // Sin ocupacion completa solo importa el primer slot: es donde iria.
                if (!fullOcupancy) return verdict;

                if (verdict != EquipResult.SuccessEquip) return verdict;
            }

            return EquipResult.SuccessEquip;
        }

        public EquipResult EquipItem(IReadOnlyList<EquipmentSlotType> slotTypes, ItemEntity item, bool fullOcupancy = false)
        {
            EquipResult verdict = CanEquip(slotTypes, item, fullOcupancy);
            if (verdict != EquipResult.SuccessEquip) return verdict;

            if (!fullOcupancy)
                return EquipmentSlots[slotTypes[0]].EquipItem(item);

            List<EquipmentSlotType> slotsWhereSucceeded = new List<EquipmentSlotType>(); // Lista de lo añadido, para poder hacer rollback en caso de ser necesario
            // Bucle para añdir si fullOcupancy == true
            foreach (EquipmentSlotType slotType in slotTypes)
            { 
                EquipResult equiped = EquipmentSlots[slotType].EquipItem(item);
                if (equiped == EquipResult.SuccessEquip)
                    slotsWhereSucceeded.Add(slotType);
                else 
                {
                    foreach (EquipmentSlotType succeededSlotType in slotsWhereSucceeded)
                        EquipmentSlots[succeededSlotType].UnequipItem(item);
                    return equiped;
                }
            }

            return EquipResult.SuccessEquip;
        }
        
        public void UnequipItem(ItemEntity item, EquipmentSlotType slotType)
        {
            AC.CheckNotNull(item, nameof(item)); 
            
            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) throw new InvalidOperationException("Cannot unequip an item with no WearableComponent.");


            EquipmentSlot slot = EquipmentSlots[slotType];
            if (!slot.UnequipItem(item)) throw new InvalidOperationException("Cannot unequip an item that is not equiped.");
            
        }

        public bool HasEquiped(ItemEntity item)
        {
             AC.CheckNotNull(item, nameof(item)); 
            
            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            if (wearableComponent == null) 
                return false;

            var equipmentSlotsList = EquipmentSlots.Values;
            foreach (EquipmentSlot slot in equipmentSlotsList)
            {
                if (wearableComponent.TargetSlots.Contains<EquipmentSlotType>(slot.SlotType))
                {
                    foreach (ItemEntity equiped in slot.Items)
                    if (equiped.Equals(item))
                        return true;
                }
                
            } 
                    
            return false;
        }

        public IComponent Clone()
        {
            EquipmentComponent equipmentComponent = new EquipmentComponent(new List<EquipmentSlotType>(_allowedSlots));
            foreach (KeyValuePair<EquipmentSlotType, EquipmentSlot> kvp in EquipmentSlots)
            {
                equipmentComponent.AddSlot(kvp.Key, kvp.Value.MaxLayers);
                List<ItemEntity> clonedItems = new List<ItemEntity>();
                foreach (ItemEntity item in kvp.Value.Items)
                {
                    clonedItems.Add((ItemEntity)item.Clone());
                }
                equipmentComponent.EquipmentSlots[kvp.Key].SetItems(clonedItems);
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

                    if (EquipmentSlots.ContainsKey(slotType) != otherEquipment.EquipmentSlots.ContainsKey(slotType))
                        return false;

                    if (EquipmentSlots.ContainsKey(slotType)) {
                        EquipmentSlot thisSlot = EquipmentSlots[slotType];
                        EquipmentSlot otherSlot = otherEquipment.EquipmentSlots[slotType];

                        if (thisSlot.GetEquippedItemCount() != otherSlot.GetEquippedItemCount() ||
                            thisSlot.MaxLayers != otherSlot.MaxLayers)
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}