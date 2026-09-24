using System.Collections.Generic;
using System.IO;
using Core;
using Core.ECS.Component;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Newtonsoft.Json;

namespace Core.Item
{
    public class JsonItemCatalogLoader
    {
        private readonly TypeIdMapper _typeIdMapper;

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
                            baseItem.SetGenericName(itemData.name);
                            if (string.IsNullOrEmpty(baseItem.Description))
                                baseItem.SetDescription(itemData.description ?? "");
                            if (string.IsNullOrEmpty(baseItem.IconPath))
                                baseItem.SetIconPath(itemData.imagePath ?? "");
                        }

                        if (component is ModelComponent model)
                        {
                            WarnAboutMissingModels(itemData.name, model);
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

        /// <summary>
        /// Avisa de las rutas de modelo que el catalogo nombra y no estan en la carpeta de
        /// datos.
        ///
        /// Se comprueba al cargar y no al dibujar porque es cuando se puede decir *que* item
        /// y *que* fichero: cuando alguien pida el modelo para ponerlo en el mundo, el fallo
        /// sera un hueco en una escena, sin nombre y sin momento. El caso tipico no es un
        /// error del exportador sino un despiste al copiar los datos: llega el data.json y se
        /// olvida la carpeta de modelos, que pesa cien veces mas.
        ///
        /// Avisa, pero no descarta el componente. Que falte un fichero hoy no invalida la
        /// regla de que etapa toca, y borrar el componente convertiria un item que se ve mal
        /// en un item sin modelo declarado, que es una mentira mas dificil de rastrear.
        /// </summary>
        private void WarnAboutMissingModels(string itemName, ModelComponent model)
        {
            foreach (ModelStage stage in model.Stages)
            {
                foreach (string path in stage.Paths)
                {
                    if (!File.Exists(CoreConfig.ResolveAsset(path)))
                    {
                        CoreLogger.Instance.LogWarning(
                            "JsonItemCatalogLoader: Item '" + itemName + "' references a missing model file: " + path);
                    }
                }
            }
        }

        private IComponent CreateComponent(ComponentData data)
        {
            IComponent instance = ItemComponentRegistry.Create(data.type);

            if (instance == null)
            {
                CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Unknown component type '" + data.type + "'. Skipping.");
                return null;
            }

            if (data.values == null || data.values.Count == 0)
            {
                CoreLogger.Instance.LogWarning("JsonItemCatalogLoader: Component '" + data.type + "' has no values. Skipping.");
                return null;
            }

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
