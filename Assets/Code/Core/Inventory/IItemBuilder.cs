using ECS.Entity;

namespace Inventory
{
    public interface IItemBuilder
    {
        public ItemEntity Build();
    }
}