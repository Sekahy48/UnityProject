using System;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.Inventory;

namespace Core.MVC.View.UI.Inventory
{
    public static class DisplayDTOsBuilder
    {
        /// <summary>
        /// Datos de una PILA entera: su representante mas lo que solo el nodo sabe de si mismo.
        ///
        /// Existe porque hay hechos que no caben en BuildDisplayData: si la pila tiene varias
        /// variantes es propiedad del nodo, no del item, y desde una ItemEntity suelta no se
        /// puede saber. Rellenarlo a mano en cada sitio que pinta un nodo condenaba al flag a
        /// olvidarse en el proximo que se anadiera.
        /// </summary>
        public static ItemDisplayData BuildNodeData(ItemObject node)
        {
            if (node == null) return null;

            ItemEntity representative = node.GetItemEntity();
            if (representative == null) return null;

            ItemDisplayData data = BuildDisplayData(representative, node.GetAmount());
            data.Sublots = node.GetSubLots.Count > 1;

            return data;
        }

        public static ItemDisplayData BuildDisplayData(ItemEntity itemEntity, int amount)
        {
            BaseItemComponent baseItem = itemEntity.GetComponent<BaseItemComponent>();
            if (baseItem == null)
                throw new InvalidOperationException(
                    $"Item '{itemEntity.GetDisplayName()}' has no BaseItemComponent");

            return new ItemDisplayData
            {
                TypeId      = baseItem.TypeId,
                Name        = itemEntity.GetDisplayName(),
                TypeName    = itemEntity.GetGenericName(),
                Amount      = amount,
                IconPath    = baseItem.IconPath,
                Description = baseItem.Description,
                Weight      = baseItem.Weight,
                Durability  = baseItem.Durability,
                DimensionW  = baseItem.DimensionW,
                DimensionH  = baseItem.DimensionH,
                IsContainer = itemEntity.HasComponent(typeof(StorageComponent))
            };
        }

        
    }
}