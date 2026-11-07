using Core.ECS.Component; 

namespace Core.ECS.Entity
{
    public class ItemEntity : InGameEntity
    {
        public ItemEntity(int id) : base(id)
        {
        }

        protected override InGameEntity CreateCloneInstance(int id)
        {
            return new ItemEntity(id);
        }

        /// <summary>
        /// Clona el item con su tipo. Reapuntar su inventario a la copia (sin eso, la mochila
        /// nueva pediria su techo de peso a la vieja) lo hace ya <see cref="InGameEntity.Clone"/>
        /// para cualquier entidad con inventario.
        /// </summary>
        public new ItemEntity Clone() => (ItemEntity)base.Clone();

        public string GetDisplayName()
        {
            NameComponent nameComp = GetComponent<NameComponent>();
            return nameComp != null ? nameComp.DisplayName : GetGenericName();
        }

        public string GetGenericName()
        {
            return GetComponent<BaseItemComponent>().GenericName;
        }
    }
}