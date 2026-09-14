using Core.ECS.Component; 

namespace Core.ECS.Entity
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

        /// <summary>
        /// Clona el item y, si lleva inventario, lo reapunta a la copia.
        ///
        /// El inventario clonado guarda una referencia a la entidad de la que cuelga, y esa
        /// referencia la copia el clon tal cual: sin reapuntarla, la mochila nueva tendria el
        /// contenido correcto pero pediria su techo de peso a la mochila vieja.
        /// </summary>
        public new ItemEntity Clone()
        {
            ItemEntity clone = (ItemEntity)base.Clone();

            InventoryComponent inventory = clone.GetComponent<InventoryComponent>();
            inventory?.Inventory.Rebind(clone);

            return clone;
        }

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