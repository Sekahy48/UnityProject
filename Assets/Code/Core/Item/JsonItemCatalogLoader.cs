using System.Collections.Generic;
using System.IO;
using Core;
using Core.ECS.Component;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Newtonsoft.Json;

namespace Core.Item
{
    /// <summary>
    /// Lee el catalogo de items exportado por Stack&amp;Go.
    ///
    /// <para><b>Un catalogo con errores no arranca el juego</b> (decidido el 2026-11-13): se
    /// lanza al cargar, con TODOS los fallos juntos, para enterarse al arrancar y no con un
    /// item que falta a mitad de partida. Lo que no se trata como error son los ficheros de
    /// modelo que faltan: eso es un despiste al copiar los datos, no un catalogo mal
    /// escrito, y solo se avisa (ver <see cref="WarnAboutMissingModels"/>).</para>
    /// </summary>
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
                throw new FileNotFoundException("JsonItemCatalogLoader: no existe el catalogo de items.", path);

            string json = File.ReadAllText(path);
            CatalogData catalogData = JsonConvert.DeserializeObject<CatalogData>(json);

            if (catalogData == null || catalogData.items == null)
                throw new InvalidDataException("JsonItemCatalogLoader: el catalogo no tiene lista de items: " + path);

            List<string> errors = new List<string>();
            int loadedCount = 0;

            for (int i = 0; i < catalogData.items.Count; i++)
            {
                ItemData itemData = catalogData.items[i];
                if (itemData == null || string.IsNullOrEmpty(itemData.name))
                {
                    errors.Add("item n.º " + i + ": sin nombre");
                    continue;
                }

                ItemEntity prototype = CreatePrototype(itemData, errors);

                if (prototype == null)
                {
                    continue;
                }

                catalogue.AddPrototype(prototype);
                loadedCount++;
            }

            if (errors.Count > 0)
                throw new InvalidDataException("JsonItemCatalogLoader: el catalogo tiene " + errors.Count +
                                               " errores (" + path + "):\n- " + string.Join("\n- ", errors));

            _typeIdMapper.Save();
            CoreLogger.Instance.Log("JsonItemCatalogLoader: cargados " + loadedCount + " items.");
        }

        private ItemEntity CreatePrototype(ItemData itemData, List<string> errors)
        {
            int typeId = _typeIdMapper.GetOrAssignId(itemData.name);
            ItemEntity prototype = new ItemEntity(IdGenerator.GenerateNewId());

            bool hasBaseItem = false;

            if (itemData.components != null)
            {
                foreach (ComponentData compData in itemData.components)
                {
                    IComponent component = CreateComponent(itemData.name, compData, errors);
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
                errors.Add("'" + itemData.name + "': no tiene componente BaseItem");
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

        private IComponent CreateComponent(string itemName, ComponentData data, List<string> errors)
        {
            IComponent instance = data == null ? null : ItemComponentRegistry.Create(data.type);

            if (instance == null)
            {
                errors.Add("'" + itemName + "': tipo de componente desconocido '" + data?.type + "'");
                return null;
            }

            if (data.values == null || data.values.Count == 0)
            {
                errors.Add("'" + itemName + "': el componente '" + data.type + "' no tiene valores");
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
