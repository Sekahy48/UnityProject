using ECS.Component;

namespace Inventory
{
    /// <summary>
    /// An item placed on the grid. Holds a reference to the composite node
    /// and its top-left position on the grid.
    /// </summary>
    public class GridElement
    {
        private readonly ItemObject _node;
        private int _row;
        private int _col;

        public GridElement(ItemObject node, int row, int col)
        {
            _node = node;
            _row = row;
            _col = col;
        }

        public ItemObject GetNode() => _node;
        public int GetRow() => _row;
        public int GetCol() => _col;

        /// <summary>
        /// Item dimensions derived from the node's entity.
        /// </summary>
        public int GetItemH() => _node.GetItemEntity().GetComponent<BaseItemComponent>().GetDimensionH();
        public int GetItemW() => _node.GetItemEntity().GetComponent<BaseItemComponent>().GetDimensionW();

        /// <summary>
        /// Updates position (for drag & drop repositioning).
        /// </summary>
        public void SetPosition(int row, int col)
        {
            _row = row;
            _col = col;
        }
    }
}
