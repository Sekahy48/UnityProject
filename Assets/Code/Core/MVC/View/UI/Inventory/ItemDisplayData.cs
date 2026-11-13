namespace Core.MVC.View.UI.Inventory
{
    public class ItemDisplayData
    {
        /// <summary>
        /// Identifies the item TYPE, shared by every instance of it. Not the stack:
        /// two piles of apples in different cells share TypeId but have different
        /// nodeIds (see ItemObject.GetNodeId).
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Name to show for this instance: the custom one if it carries a NameComponent,
        /// the generic one otherwise. Comes from ItemEntity.GetDisplayName().
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the item type, always the prototype's. Comes from
        /// ItemEntity.GetGenericName(), ignoring any NameComponent.
        /// Use this wherever the type matters rather than the instance —
        /// the dev catalog, for example, lists prototypes.
        /// </summary>
        public string TypeName { get; set; }

        public int Amount { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }
        /// <summary>Peso de una unidad aislada: el <c>BaseItemComponent.Weight</c>. En un
        /// contenedor, la mochila vacia.</summary>
        public float UnitWeight { get; set; }

        /// <summary>
        /// Lo que pesan todas las unidades juntas, contenido incluido si es un contenedor.
        ///
        /// Se calcula en Core con <c>WeightCalculator</c> y no en la vista multiplicando: la vista
        /// hacia <c>Weight * Amount</c> y una mochila llena salia con el peso de vacia. Es la
        /// misma cifra con la que el inventario decide si algo cabe, asi que la tira no puede
        /// decir una cosa y el fantasma otra.
        /// </summary>
        public float TotalWeight { get; set; }
        public float Durability { get; set; }
        public int DimensionW { get; set; }
        public int DimensionH { get; set; }
        public bool IsContainer { get; set; }
        public int TabIndex { get; set; } // if IsContainer, which tab to navigate to on click
        public bool Sublots { get; set; }
    }
}