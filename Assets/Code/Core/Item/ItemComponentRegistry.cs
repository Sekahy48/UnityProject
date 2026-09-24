using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Component.InventoryComponents;
using Core.ECS.Component.ItemComponents;

namespace Core.Item
{
    /// <summary>
    /// Unico sitio que sabe traducir el nombre con el que un componente viaja en el catalogo
    /// —<c>"Material"</c>, <c>"Wearable"</c>— en algo del dominio.
    ///
    /// Antes ese conocimiento vivia dentro del cargador de catalogo, que lo necesitaba para
    /// fabricar componentes al leer el JSON. En cuanto aparecio un segundo interesado —la
    /// lectura de magnitudes, que necesita el mismo nombre para *encontrar* un componente ya
    /// puesto— habria hecho falta un segundo mapa con exactamente las mismas claves. Dos
    /// listas que tienen que coincidir y que nada obliga a coincidir: anadir un componente y
    /// registrarlo en una sola es cuestion de tiempo.
    ///
    /// Un componente que no este aqui simplemente no existe para el catalogo: ni se carga
    /// desde el JSON ni se puede nombrar como magnitud.
    /// </summary>
    public static class ItemComponentRegistry
    {
        private class Entry
        {
            public Type Type;
            public Func<IComponent> Factory;
        }

        private static readonly Dictionary<string, Entry> Entries = new Dictionary<string, Entry>();

        static ItemComponentRegistry()
        {
            Register<BaseItemComponent>("BaseItem");
            Register<MaterialComponent>("Material");
            Register<DamageComponent>("Damage");
            Register<StorageComponent>("Storage");
            Register<NutritionComponent>("Nutrition");
            Register<FluidComponent>("Fluid");
            Register<HealComponent>("Heal");
            Register<NameComponent>("Name");
            Register<ResourceComponent>("Resource");
            Register<WearableComponent>("Wearable");
            Register<ModelComponent>("Model");
        }

        /// <summary>
        /// El nombre del tipo aparece una sola vez por linea gracias al generico. Con un
        /// diccionario literal habria que escribirlo dos veces —el Type y el new— y las dos
        /// podrian dejar de coincidir.
        /// </summary>
        private static void Register<T>(string name) where T : IComponent, new()
        {
            Entries[name] = new Entry { Type = typeof(T), Factory = () => new T() };
        }

        public static bool Contains(string name)
            => name != null && Entries.ContainsKey(name);

        /// <summary>Crea un componente vacio de ese nombre, o null si no esta registrado.</summary>
        public static IComponent Create(string name)
            => name != null && Entries.TryGetValue(name, out Entry entry) ? entry.Factory() : null;

        /// <summary>
        /// Tipo con el que pedirle ese componente a una entidad, o null si no esta
        /// registrado.
        /// </summary>
        public static Type TypeOf(string name)
            => name != null && Entries.TryGetValue(name, out Entry entry) ? entry.Type : null;

        public static IEnumerable<string> Names => Entries.Keys;
    }
}
