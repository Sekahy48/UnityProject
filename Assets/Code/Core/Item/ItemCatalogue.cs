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

        public IEnumerable<ItemEntity> GetAll()
        {
            return prototipes.Values;
        }

        public void LogCatalogContents()
        {
            CoreLogger.Instance.Log($"=== ItemCatalogue: {prototipes.Count} prototypes ===");

            foreach (var kvp in prototipes)
            {
                ItemEntity proto = kvp.Value;
                BaseItemComponent baseItem = proto.GetComponent<BaseItemComponent>();

                string name = baseItem != null ? baseItem.GetGenericName() : "???";
                string line = $"  [{kvp.Key}] {name}";

                // Listar componentes
                List<string> compNames = new List<string>();
                foreach (IComponent comp in proto.GetComponents())
                {
                    string compName = comp.GetType().Name;
                    if (comp is BaseItemComponent) continue; // ya sale arriba
                    compNames.Add(compName);
                }

                if (compNames.Count > 0)
                    line += " | " + string.Join(", ", compNames);

                CoreLogger.Instance.Log(line);
            }

            CoreLogger.Instance.Log("=== End of catalog ===");
        }
    }
}