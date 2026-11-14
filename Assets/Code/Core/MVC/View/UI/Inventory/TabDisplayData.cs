namespace Core.MVC.View.UI.Inventory
{
    public class TabDisplayData
    {
        public int Index { get; set; }
        public string Label { get; set; }
        public string IconPath { get; set; }
        public bool IsEquipment { get; set; }  // tab 1
        public bool IsBaseInventory { get; set; } // tab 2
    }
}