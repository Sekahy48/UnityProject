using System.Collections.Generic;
using Core.MVC.View.UI.Inventory;

namespace Core.MVC.View.UI.World
{
    /// <summary>
    /// Lo que muestra el panel de inspeccion de algo del mundo.
    ///
    /// Reutiliza <see cref="ItemDisplayData"/>, el mismo DTO de la tira de inspeccion del
    /// inventario: un item se describe igual este donde este. Lo unico propio del panel es
    /// el desglose por variantes.
    /// </summary>
    public class InspectPanelData
    {
        /// <summary>El conjunto: nombre, icono y descripcion del representante; cantidad y
        /// peso total de todo.</summary>
        public ItemDisplayData Item;

        /// <summary>Una entrada por variante. Vacia o de un solo elemento: no hay desglose
        /// que mostrar.</summary>
        public IReadOnlyList<ItemDisplayData> Lots;
    }
}
