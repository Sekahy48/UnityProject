using Inventory;

namespace ECS.Component
{
    public class InventoryComponent : BasicComponent
    {
        private IInventoryElement _inventory;

        public InventoryComponent(int n)
        {

        }
        
        public override IComponent Clone()
        {
            return new InventoryComponent(0);
        }
    }
}