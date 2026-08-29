namespace MVC.View.UI.Inventory
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
        public float Weight;
        public float Durability;
        public int DimensionW;
        public int DimensionH;
        public bool IsContainer;
        public int TabIndex; // if IsContainer, which tab to navigate to on click
    }
}