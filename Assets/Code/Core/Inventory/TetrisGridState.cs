using System;
using System.Collections.Generic;
using ECS.Component;

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
        /// Returns the nodeId at the given cell, or -1 if free.
        /// </summary>
        public int GetCellAt(int row, int col) => _cells[row, col];

        /// <summary>
        /// Returns the GridElement placed at the given cell, or null if free.
        /// </summary>
        public GridElement GetElementAt(int row, int col)
        {
            int nodeId = _cells[row, col];
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
        public bool CanPlace(int row, int col, int itemH, int itemW)
        {
            if (row < 0 || col < 0) return false;
            if (row + itemH > _gridH || col + itemW > _gridW) return false;

            for (int r = row; r < row + itemH; r++)
                for (int c = col; c < col + itemW; c++)
                    if (_cells[r, c] != -1) return false;

            return true;
        }

        /// <summary>
        /// Places a node at the specified position. Returns false if the space is not available.
        /// </summary>
        public bool Place(ItemObject node, int row, int col)
        {
            BaseItemComponent baseItem = node.GetItemEntity().GetComponent<BaseItemComponent>();
            int itemH = baseItem.GetDimensionH();
            int itemW = baseItem.GetDimensionW();

            if (!CanPlace(row, col, itemH, itemW)) return false;

            int nodeId = node.GetNodeId();
            for (int r = row; r < row + itemH; r++)
                for (int c = col; c < col + itemW; c++)
                    _cells[r, c] = nodeId;

            _elements.Add(new GridElement(node, row, col));
            return true;
        }

        public bool TryFirstPlace(ItemObject node)
        { 
            BaseItemComponent baseInfo = node.GetItemEntity().GetComponent<BaseItemComponent>();
            (int row, int col) coords = FindFirstFit(baseInfo.GetDimensionH(), baseInfo.GetDimensionW());
            return Place(node, coords.row, coords.col);
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
        /// Returns (row, col) or (-1, -1) if no space.
        /// </summary>
        public (int row, int col) FindFirstFit(int itemH, int itemW)
        {
            for (int r = 0; r <= _gridH - itemH; r++)
                for (int c = 0; c <= _gridW - itemW; c++)
                    if (CanPlace(r, c, itemH, itemW)) return (r, c);

            return (-1, -1);
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
