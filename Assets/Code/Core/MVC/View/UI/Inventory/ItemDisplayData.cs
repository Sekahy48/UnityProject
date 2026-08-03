namespace MVC.View.UI.Inventory
{
    public class ItemDisplayData
    {
        public int Id;
        public string Name;
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