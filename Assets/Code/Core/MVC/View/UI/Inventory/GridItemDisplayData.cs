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

        /// <summary>
        /// This node is the source of what the hand is holding. Nothing has left the grid
        /// yet, so it is still drawn — dimmed, to show where the units came from.
        /// </summary>
        public bool IsGrabbed;
    }
}
