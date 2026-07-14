using System;
using System.Collections.Generic;
using System.IO;
using Core;
using ECS.Component;
using ECS.Component.InventoryComponents;
using ECS.Entity;
using Newtonsoft.Json;

namespace Item
{
    public class JsonItemCatalogLoader
    {
        private readonly TypeIdMapper _typeIdMapper;

        private readonly Dictionary<string, Func<IComponent>> _componentRegistry = new Dictionary<string, Func<IComponent>>
        {
            { "BaseItem", () => new BaseItemComponent() },
            { "Material", () => new MaterialComponent() },
            { "Damage", () => new DamageComponent() },
            { "Storage", () => new StorageComponent() },
            { "Nutrition", () => new NutritionComponent() },
            { "Fluid", () => new FluidComponent() },
            { "Heal", () => new HealComponent() },
            { "Name", () => new NameComponent() },
            { "Resource", () => new ResourceComponent() }
        };

        public JsonItemCatalogLoader()
        {
            _typeIdMapper = new TypeIdMapper();
        }

        public void LoadInto(ItemCatalogue catalogue)
        {
            string path = CoreConfig.CatalogPath;

            if (!File.Exists(path))
            {
                CoreLogger.Instance.LogError("JsonItemCatalogLoader: Catalog file not found at " + path);
                return;
            }

            string json = File.ReadAllText(path);
            CatalogData catalogData = JsonConvert.DeserializeObject<CatalogData>(json);

            if (catalogData == null || catalogData.items == null)
            {
                CoreLogger.Instance.LogError("JsonItemCatalogLoader: Failed to parse catalog JSON.");
                return;
            }

            int loadedCount = 0;

            foreach (ItemData itemData in catalogData.items)
            {
                if (string.IsNullOrEmpty(itemData.name))
                {
                    CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Skipping item with empty name.");
                    continue;
                }

                ItemEntity prototype = CreatePrototype(itemData);

                if (prototype == null)
                {
                    continue;
                }

                catalogue.AddPrototype(prototype);
                loadedCount++;
            }

            _typeIdMapper.Save();
            CoreLogger.Instance.Log("JsonItemCatalogLoader: Loaded " + loadedCount + " items.");
        }

        private ItemEntity CreatePrototype(ItemData itemData)
        {
            int typeId = _typeIdMapper.GetOrAssignId(itemData.name);
            ItemEntity prototype = new ItemEntity(IdGenerator.GenerateNewId());

            bool hasBaseItem = false;

            if (itemData.components != null)
            {
                foreach (ComponentData compData in itemData.components)
                {
                    IComponent component = CreateComponent(compData);
                    if (component != null)
                    {
                        prototype.AddComponent(component);

                        if (component is BaseItemComponent baseItem)
                        {
                            hasBaseItem = true;
                            baseItem.SetTypeId(typeId);
                            if (string.IsNullOrEmpty(baseItem.GetDescription()))
                                baseItem.SetDescription(itemData.description ?? "");
                            if (string.IsNullOrEmpty(baseItem.GetIconPath()))
                                baseItem.SetIconPath(itemData.imagePath ?? "");
                        }
                    }
                }
            }

            if (!hasBaseItem)
            {
                CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Item '" + itemData.name + "' has no BaseItemComponent. Skipping.");
                return null;
            }

            return prototype;
        }

        private IComponent CreateComponent(ComponentData data)
        {
            if (!_componentRegistry.TryGetValue(data.type, out Func<IComponent> factory))
            {
                CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Unknown component type '" + data.type + "'. Skipping.");
                return null;
            }

            if (data.values == null || data.values.Count == 0)
            {
                CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Component '" + data.type + "' has no values. Skipping.");
                return null;
            }

            IComponent instance = factory();

            if (instance is IJsonLoadable loadable)
            {
                loadable.SetFromValues(data.values);
            }

            return instance;
        }

        // ---- Private DTOs ----

        private class CatalogData
        {
            public string collection;
            public List<ItemData> items;
        }

        private class ItemData
        {
            public string name;
            public string description;
            public string imagePath;
            public List<ComponentData> components;
        }

        private class ComponentData
        {
            public string type;
            public Dictionary<string, object> values;
        }
    }
}
