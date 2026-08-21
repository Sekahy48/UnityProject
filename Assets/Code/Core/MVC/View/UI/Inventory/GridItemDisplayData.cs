namespace MVC.View.UI.Inventory
{
    /// <summary>
    /// An item together with its placement in the tetris grid.
    /// Composes ItemDisplayData instead of extending it, so grid coordinates
    /// don't leak into the contexts where items have no position
    /// (inspection strip, equipment sub-slots).
    /// </summary>
    public class GridItemDisplayData
    {
        public ItemDisplayData Item;
        public int Row;
        public int Col;
    }
}
