namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Las ventanas de inventario: la del jugador (principal) y los dos huecos laterales.
    /// Vive en Core porque la usan los presentadores y la interfaz IContainerPanels; antes
    /// vivia en InventoryView y Core dependia de un fichero de Unity para nombrarla.
    /// </summary>
    public enum PanelType
    {
        Player,
        A,
        B,
    }
}
