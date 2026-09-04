using System;
using System.Collections.Generic;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    /// <summary>
    /// Una prenda puesta, como origen de un agarre o de una transferencia.
    ///
    /// Guarda la instancia y no su capa: el indice se mueve en cuanto alguien equipa o
    /// quita algo por encima. Y guarda una LISTA de slots porque una prenda de ocupacion
    /// completa (un arco a dos manos) esta en varios a la vez y sale de todos.
    ///
    /// Trabaja contra el componente, no contra EquipmentSystem, para no anunciar nada a
    /// mitad de transaccion: quien orquesta avisa al terminar.
    /// </summary>
    public class EquipmentSlotOrigin : IGrabOrigin
    {
        private readonly IEntity _owner;
        private readonly List<EquipmentSlotType> _slotTypes;
        private readonly ItemEntity _item;
        private readonly EquipmentSystem _system;

        public EquipmentSlotOrigin(IEntity owner, List<EquipmentSlotType> slotTypes, ItemEntity item, EquipmentSystem system)
        {
            AC.CheckNotNull(owner, nameof(owner));
            AC.CheckNotNull(item, nameof(item));
            AC.CheckNotNull(slotTypes, nameof(slotTypes));
            AC.CheckNotNull(system, nameof(system));
 
            if (slotTypes.Count == 0)
                throw new ArgumentException("Un origen de equipo necesita al menos un slot.", nameof(slotTypes));

            _owner = owner;
            _slotTypes = slotTypes;
            _item = item;
            _system = system;
        }

        public IEntity Owner => _owner;
        public ItemEntity Representative => _item;

        /// <summary>Una prenda no ocupa celdas de ninguna rejilla.</summary>
        public int SourceNodeId => -1;

        public IReadOnlyList<EquipmentSlotType> SlotTypes => _slotTypes;

        /// <summary>El equipo no apila: la prenda esta puesta o no lo esta.</summary>
        public int Available(ItemEntity variant = null) => IsEquipped() ? 1 : 0;

        public IReadOnlyList<SubLot> Extract(ItemEntity variant, int amount)
        {
            if (amount <= 0 || !IsEquipped()) return new List<SubLot>();

            
            _system.TryUnequip(_owner, _item, _slotTypes);

            return new List<SubLot> { new SubLot(_item, 1) };
        }

        public void Restore(ItemEntity variant, int amount)
        {
            if (amount <= 0) return;

            WearableComponent wearable = _item.GetComponent<WearableComponent>();
            EquipResult result = _system.TryEquip(Owner, _item, _slotTypes);

            if (result != EquipResult.SuccessEquip)
                throw new InvalidOperationException(
                    $"No se pudo devolver '{_item.GetDisplayName()}' a su equipo: {result.GetMessage()}. " +
                    "La prenda ha salido del equipo sin llegar a ningun destino.");
        }

        /// <summary>El equipo no tiene celdas que liberar.</summary>
        public void Clean()
        { 
        }

        private EquipmentComponent Equipment() => _owner.GetComponent<EquipmentComponent>();

        private bool IsEquipped()
        {
            foreach (EquipmentSlotType slotType in _slotTypes)
                foreach (ItemEntity equipped in Equipment().GetEquipmentSlot(slotType).Items)
                    if (ReferenceEquals(equipped, _item)) return true;

            return false;
        }
    }
}
