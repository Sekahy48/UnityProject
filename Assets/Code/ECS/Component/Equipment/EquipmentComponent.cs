using System;
using System.Collections.Generic; 
using ECS.Entity;
using UnityEngine;
using UnityEngine.TestTools;
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
        }

        public bool AddSlot(EquipmentSlotType slotType, int capacity)
        {   
            ArgumentChecker.CheckNotNull(slotType, nameof(slotType));
            ArgumentChecker.CheckPositive(capacity, nameof(capacity));

            bool added = false;

            if (!allowedSlots.Contains(slotType))
            {
                Debug.Log("Slot type not allowed."); 
            }

            if (equipmentSlots.ContainsKey(slotType))
            {
                Debug.Log("Slot type already exists.");  
            } else
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
            {
                Debug.Log("Slot type not allowed."); 
            } else if (!equipmentSlots.ContainsKey(slotType))
            {
                Debug.Log("This entity doesnt contains a slot of type: " + slotType);
            } else {
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
                equipmentComponent.equipmentSlots[kvp.Key].SetItems(new List<ItemEntity>(kvp.Value.GetEquippedItemCount()));
            }
            return equipmentComponent;

            //TODO revisar por si falta algo de copia profunda
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