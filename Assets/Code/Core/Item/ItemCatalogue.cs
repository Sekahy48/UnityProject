using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Entity;
using Utils;

namespace Item
{
    public class ItemCatalogue
    {
        private readonly Dictionary<int, ItemEntity> prototipes;

        public ItemCatalogue()
        {
            prototipes = new Dictionary<int, ItemEntity>();
        }

        public void AddPrototype(ItemEntity item)
        {
            ArgumentChecker.CheckNotNull(item, nameof(item));
            int typeId = item.GetComponent<BaseItemComponent>().GetTypeId();
            
            if (prototipes.ContainsKey(typeId)) CoreLogger.Instance.LogWarning($"ItemCatalogue: Prototype with typeId {typeId} already exists. Overwriting.");
            
            prototipes[typeId] = item;
        }

        public ItemEntity CreateItem(int typeId)
        {
            if (!prototipes.TryGetValue(typeId, out ItemEntity prototype))
            {
                CoreLogger.Instance.LogError($"ItemCatalogue: Prototype with typeId {typeId} not found.");
                return null;
            }

            return prototype.Clone();
        }


    }
}