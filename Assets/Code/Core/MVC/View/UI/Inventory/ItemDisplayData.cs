namespace Core.MVC.View.UI.Inventory
{
    public class ItemDisplayData
    {
        /// <summary>
        /// Identifies the item TYPE, shared by every instance of it. Not the stack:
        /// two piles of apples in different cells share TypeId but have different
        /// nodeIds (see ItemObject.GetNodeId).
        /// </summary>
        public int TypeId;

        /// <summary>
        /// Name to show for this instance: the custom one if it carries a NameComponent,
        /// the generic one otherwise. Comes from ItemEntity.GetDisplayName().
        /// </summary>
        public string Name;

        /// <summary>
        /// Name of the item type, always the prototype's. Comes from
        /// ItemEntity.GetGenericName(), ignoring any NameComponent.
        /// Use this wherever the type matters rather than the instance —
        /// the dev catalog, for example, lists prototypes.
        /// </summary>
        public string TypeName;

        public int Amount;
        public string IconPath;
        public string Description;
        /// <summary>Peso de una unidad aislada: el <c>BaseItemComponent.Weight</c>. En un
        /// contenedor, la mochila vacia.</summary>
        public float UnitWeight;

        /// <summary>
        /// Lo que pesan todas las unidades juntas, contenido incluido si es un contenedor.
        ///
        /// Se calcula en Core con <c>ItemWeight</c> y no en la vista multiplicando: la vista
        /// hacia <c>Weight * Amount</c> y una mochila llena salia con el peso de vacia. Es la
        /// misma cifra con la que el inventario decide si algo cabe, asi que la tira no puede
        /// decir una cosa y el fantasma otra.
        /// </summary>
        public float TotalWeight;
        public float Durability;
        public int DimensionW;
        public int DimensionH;
        public bool IsContainer;
        public int TabIndex; // if IsContainer, which tab to navigate to on click
        public bool Sublots;
    }
}