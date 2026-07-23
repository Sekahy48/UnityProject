using System.Runtime.CompilerServices;
using Inventory;

namespace ECS.Component
{
    public class InventoryComponent : BasicComponent
    {
        private InventoryObject _inventory;

        public InventoryComponent(InventoryObject inventory)
        {
            this._inventory = inventory;
        }

        public InventoryObject Inventory => _inventory;

        public override IComponent Clone()
        {
            return new InventoryComponent((InventoryObject)this._inventory.Clone());
        }

        public override bool Equivalent(IComponent other)
        {
            return
                other is InventoryComponent otherInventory &&
                this._inventory.Equivalent(otherInventory._inventory);
        }
 
    }
}