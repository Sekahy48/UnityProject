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
        private GridPos _pos;

        public GridElement(ItemObject node, GridPos pos)
        {
            _node = node;
            _pos = pos;
        }

        public ItemObject GetNode() => _node;
        public GridPos GetPos() => _pos;

        /* Atajos para quien solo necesita una de las dos coordenadas (los DTO de pintado,
           por ejemplo) sin desempaquetar la celda entera. */
        public int GetRow() => _pos.Row;
        public int GetCol() => _pos.Col;

        /// <summary>
        /// Item dimensions derived from the node's entity.
        /// </summary>
        public int GetItemH() => _node.GetItemEntity().GetComponent<BaseItemComponent>().DimensionH;
        public int GetItemW() => _node.GetItemEntity().GetComponent<BaseItemComponent>().DimensionW;

        /// <summary>
        /// Updates position (for drag & drop repositioning).
        /// </summary>
        public void SetPosition(GridPos pos) => _pos = pos;
    }
}
