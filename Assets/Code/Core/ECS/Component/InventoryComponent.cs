using System.Runtime.CompilerServices;
using Inventory;

namespace ECS.Component
{
    public class InventoryComponent : BasicComponent
    {
        private IInventoryElement _inventory;  
         
        public InventoryComponent(IInventoryElement inventory)
        {
            this._inventory = inventory;  
        }

        public IInventoryElement Inventory => _inventory;
        
        public override IComponent Clone()
        {
            return new InventoryComponent(this._inventory.Clone());
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is InventoryComponent otherInventory &&
                this._inventory.Equivalent(otherInventory._inventory);
        }
 
    }
}