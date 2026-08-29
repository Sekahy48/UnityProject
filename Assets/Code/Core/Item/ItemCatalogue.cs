using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace Item
{
    public class ItemCatalogue
    {
        private readonly Dictionary<int, ItemEntity> _prototypes;

        public ItemCatalogue()
        {
            _prototypes = new Dictionary<int, ItemEntity>();
        }

        public void AddPrototype(ItemEntity item)
        {
            AC.CheckNotNull(item, nameof(item));
            int typeId = item.GetComponent<BaseItemComponent>().TypeId;
            
            if (_prototypes.ContainsKey(typeId)) CoreLogger.Instance.LogWarning($"ItemCatalogue: Prototype with typeId {typeId} already exists. Overwriting.");
            
            _prototypes[typeId] = item;
        }

        public ItemEntity CreateItem(int typeId)
        {
            if (!_prototypes.TryGetValue(typeId, out ItemEntity prototype))
            {
                throw new KeyNotFoundException($"ItemCatalogue: Prototype with typeId {typeId} not found.");
            }

            return prototype.Clone();
        }

        public ItemEntity CreateItem(string name) => CreateItem(GetTypeIdByName(name));
        
        public int GetTypeIdByName(string name)
        {
            AC.CheckNotNull(name, nameof(name));
            
            foreach (ItemEntity proto in _prototypes.Values)
                if (proto.GetGenericName() == name)
                    return proto.GetComponent<BaseItemComponent>().TypeId;
            throw new KeyNotFoundException($"ItemCatalogue: Prototype with name '{name}' not found. There is no matching key/typeId");
        }

        public IEnumerable<ItemEntity> GetAll()
        {
            return _prototypes.Values;
        }

        public void LogCatalogContents()
        {
            CoreLogger.Instance.Log($"=== ItemCatalogue: {_prototypes.Count} prototypes ===");

            foreach (var kvp in _prototypes)
            {
                ItemEntity proto = kvp.Value;
                BaseItemComponent baseItem = proto.GetComponent<BaseItemComponent>();

                string name = baseItem != null ? baseItem.GenericName : "???";
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