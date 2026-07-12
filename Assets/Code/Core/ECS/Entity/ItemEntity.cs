namespace ECS.Entity
{
    public class ItemEntity : InGameEntity
    {
        public ItemEntity(int id, string type) : base(id, type)
        {
        }

        protected override InGameEntity CreateCloneInstance(int id, string type)
        {
            return new ItemEntity(id, type);
        }

        public new ItemEntity Clone()
        {
            return (ItemEntity)base.Clone();
        }
    }
}