using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// Engancha y desengancha el inventario de una prenda equipable del arbol de quien la
    /// lleva.
    ///
    /// Vive aparte porque hay CUATRO caminos que equipan o desequipan —el menu contextual, la
    /// mano sobre un slot, el desequipado por menu y el agarre de una capa— y la regla tiene
    /// que ser la misma en los cuatro. Repartida en copias ya fallo una vez: equipar desde el
    /// menu enganchaba y arrastrar al slot no, asi que una mochila arrastrada no pesaba.
    ///
    /// <para>La regla en si: mientras se lleva puesta, su inventario cuelga del de quien la
    /// lleva. De ahi sale todo lo demas — el peso sube por el arbol y la capacidad se
    /// pregunta hacia arriba — sin que nadie tenga que anunciar nada.</para>
    /// </summary>
    public static class WornContainers
    {
        /// <summary>Cuelga el inventario de la prenda del de quien la lleva.</summary>
        /// <returns>False si la prenda no es un contenedor, o ya estaba enganchada ahi.</returns>
        public static bool Attach(IEntity wearer, ItemEntity garment)
        {
            InventoryObject worn = InventoryOf(garment);
            InventoryObject host = InventoryOf(wearer);
            if (worn == null || host == null) return false;

            // Idempotente a proposito: el equipado y su vuelta atras pueden pasar los dos por
            // aqui, y colgar dos veces el mismo contenedor lo contaria dos veces.
            if (ReferenceEquals(worn.Parent, host)) return false;

            host.AddContainer(worn);

            return true;
        }

        /// <summary>Descuelga el inventario de la prenda. El reverso exacto de Attach.</summary>
        /// <returns>False si la prenda no es un contenedor, o no colgaba de ahi.</returns>
        public static bool Detach(IEntity wearer, ItemEntity garment)
        {
            InventoryObject worn = InventoryOf(garment);
            InventoryObject host = InventoryOf(wearer);
            if (worn == null || host == null) return false;

            return host.RemoveContainer(worn);
        }

        private static InventoryObject InventoryOf(IEntity entity)
            => entity?.GetComponent<InventoryComponent>()?.Inventory;
    }
}
