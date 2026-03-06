namespace MVC.View.UI.Inventory
{
    public class ItemDisplayData
    {
        public string Id;
        public string Name;
        public int Amount;
        public string IconPath;
        public bool IsContainer;
        public int TabIndex; // si IsContainer, a qué tab navega al clickar
    }
}