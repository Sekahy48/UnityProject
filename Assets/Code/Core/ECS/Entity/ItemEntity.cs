namespace ECS.Entity
{
    public class ItemEntity : InGameEntity
    {
        public ItemEntity(int id) : base(id, "ItemEntity")
        {
        }

        protected override InGameEntity CreateCloneInstance(int id, string type)
        {
            return new ItemEntity(id);
        }

        public new ItemEntity Clone()
        {
            return (ItemEntity)base.Clone();
        }
    }
}