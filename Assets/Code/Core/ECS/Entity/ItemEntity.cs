using ECS.Component;

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

        public string GetDisplayName()
        {

            string displayName;

            if (this.GetComponent<NameComponent>() != null)
            {
                displayName = this.GetComponent<NameComponent>().DisplayName;  
            }
            else
            {
                displayName = this.GetComponent<BaseItemComponent>().GetGenericName();
            }

            return displayName;
        }
    }
}