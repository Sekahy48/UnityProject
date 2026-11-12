using System.Runtime.CompilerServices;
using Core.Inventory;

namespace Core.ECS.Component
{
    public class InventoryComponent : BasicComponent
    {
        private InventoryObject _inventory;

        public InventoryComponent(InventoryObject inventory)
        {
            _inventory = inventory;
        }

        public InventoryObject Inventory => _inventory;

        public override IComponent Clone()
        {
            return new InventoryComponent((InventoryObject)_inventory.Clone());
        }

        public override bool Equivalent(IComponent other)
        {
            return
                other is InventoryComponent otherInventory &&
                _inventory.Equivalent(otherInventory._inventory);
        }
 
    }
}