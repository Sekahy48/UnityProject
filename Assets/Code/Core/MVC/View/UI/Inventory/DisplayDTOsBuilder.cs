using System;
using ECS.Component;
using ECS.Entity;

namespace MVC.View.UI.Inventory
{
    public static class DisplayDTOsBuilder
    {
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