using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;

namespace Inventory
{
    /// <summary>
    /// Tracks which cells are occupied in a grid-based inventory.
    /// Dual representation: list of placed elements + 2D matrix for O(1) cell queries.
    /// </summary>
    public class TetrisGridState
    {
        private readonly int _gridH;
        private readonly int _gridW;
        private readonly int[,] _cells; // nodeId or -1 if free
        private readonly List<GridElement> _elements;

        public TetrisGridState(int gridH, int gridW)
        {
            _gridH = gridH;
            _gridW = gridW;
            _cells = new int[gridH, gridW];
            _elements = new List<GridElement>();

            for (int r = 0; r < gridH; r++)
                for (int c = 0; c < gridW; c++)
                    _cells[r, c] = -1;
        }

        //#region Getters

        public int GetGridH() => _gridH;
        public int GetGridW() => _gridW;
        public List<GridElement> GetElements() => new List<GridElement>(_elements);

        /// <summary>
        /// Whether the given cell exists in this grid.
        /// </summary>
        public bool IsInside(GridPos pos) =>
            pos.Row >= 0 && pos.Col >= 0 && pos.Row < _gridH && pos.Col < _gridW;

        /// <summary>
        /// Returns the nodeId at the given cell, or -1 if free or outside the grid.
        /// </summary>
        public int GetCellAt(GridPos pos) => IsInside(pos) ? _cells[pos.Row, pos.Col] : -1;

        /// <summary>
        /// Returns the GridElement placed at the given cell, or null if free or outside
        /// the grid.
        /// </summary>
        public GridElement GetElementAt(GridPos pos)
        {
            int nodeId = GetCellAt(pos);
            if (nodeId == -1) return null;

            foreach (GridElement elem in _elements)
                if (elem.GetNode().GetNodeId() == nodeId) return elem;

            return null;
        }

        //#endregion

        //#region Placement

        /// <summary>
        /// Checks if an item with the given dimensions can be placed at the specified position.
        /// </summary>
        /// <param name="ignoreNodeId">
        /// Node whose cells count as free. Needed to move a node onto a position overlapping
        /// its own: the hand holds a reference and nothing leaves the grid until it is placed,
        /// so the node's cells still hold its id and would block it against itself. Nudging a
        /// 1x3 blade one row down is the common case.
        /// -1 blocks nothing extra, since NodeIdGenerator starts at 1.
        /// </param>
        public bool CanPlace(GridPos pos, int itemH, int itemW, int ignoreNodeId = -1)
        {
            if (pos.Row < 0 || pos.Col < 0) return false;
            if (pos.Row + itemH > _gridH || pos.Col + itemW > _gridW) return false;

            for (int r = pos.Row; r < pos.Row + itemH; r++)
                for (int c = pos.Col; c < pos.Col + itemW; c++)
                    if (_cells[r, c] != -1 && _cells[r, c] != ignoreNodeId) return false;

            return true;
        }

        public bool CanPlace(ItemEntity item)
        {
            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            return !FindFirstFit(baseInfo.DimensionH, baseInfo.DimensionW).IsNone;
        }

        /// <summary>
        /// Places a node at the specified position. Returns false if the space is not available.
        /// A node may overlap its own previous cells, so this doubles as "move here": the old
        /// cells are released first, otherwise they would stay marked with its id and the node
        /// would hold more space than it occupies.
        /// </summary>
        /// <param name="ignoreNodeId">
        /// Extra node whose cells count as free, on top of this node's own — see CanPlace.
        /// Needed when the move is done by building a NEW node and dropping the old one: the
        /// new id owns no cell yet, so without this the placement collides with the node it
        /// is replacing.
        /// </param>
        public bool Place(ItemObject node, GridPos pos, int ignoreNodeId = -1)
        {
            BaseItemComponent baseItem = node.GetItemEntity().GetComponent<BaseItemComponent>();
            int itemH = baseItem.DimensionH;
            int itemW = baseItem.DimensionW;

            if (!CanPlace(pos, itemH, itemW, node.GetNodeId())
             && !CanPlace(pos, itemH, itemW, ignoreNodeId)) return false;

            int nodeId = node.GetNodeId();
            Remove(nodeId);   // no-op if it wasn't placed yet

            for (int r = pos.Row; r < pos.Row + itemH; r++)
                for (int c = pos.Col; c < pos.Col + itemW; c++)
                    _cells[r, c] = nodeId;

            _elements.Add(new GridElement(node, pos));
            return true;
        }

        public bool TryFirstPlace(ItemObject node)
        { 
            BaseItemComponent baseInfo = node.GetItemEntity().GetComponent<BaseItemComponent>();
            GridPos pos = FindFirstFit(baseInfo.DimensionH, baseInfo.DimensionW);
            return !pos.IsNone && Place(node, pos);
        }

        /// <summary>
        /// Removes a node from the grid by its nodeId.
        /// </summary>
        public bool Remove(int nodeId)
        {
            GridElement toRemove = null;
            foreach (GridElement elem in _elements)
            {
                if (elem.GetNode().GetNodeId() == nodeId)
                {
                    toRemove = elem;
                    break;
                }
            }

            if (toRemove == null) return false;

            for (int r = 0; r < _gridH; r++)
                for (int c = 0; c < _gridW; c++)
                    if (_cells[r, c] == nodeId) _cells[r, c] = -1;

            _elements.Remove(toRemove);
            return true;
        }

        //#endregion

        //#region Queries

        /// <summary>
        /// Finds the first position where an item with the given dimensions fits.
        /// Scans left-to-right, top-to-bottom.
        /// Returns the cell, or GridPos.None if no space.
        /// </summary>
        /// <param name="ignoreNodeId">Node whose cells count as free — see CanPlace.</param>
        public GridPos FindFirstFit(int itemH, int itemW, int ignoreNodeId = -1)
        {
            for (int r = 0; r <= _gridH - itemH; r++)
                for (int c = 0; c <= _gridW - itemW; c++)
                {
                    GridPos pos = new GridPos(r, c);
                    if (CanPlace(pos, itemH, itemW, ignoreNodeId)) return pos;
                }

            return GridPos.None;
        }

        /// <summary>
        /// Returns the number of free cells in the grid.
        /// </summary>
        public int GetFreeCellCount()
        {
            int free = 0;
            for (int r = 0; r < _gridH; r++)
                for (int c = 0; c < _gridW; c++)
                    if (_cells[r, c] == -1) free++;
            return free;
        }

        //#endregion
    }
}
