using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// Lleva la cuenta de que celdas estan ocupadas en un inventario de rejilla.
    /// Doble representacion: lista de elementos colocados + matriz 2D para consultar una celda en O(1).
    /// </summary>
    public class TetrisGridState
    {
        private readonly int _gridH;
        private readonly int _gridW;
        private readonly int[,] _cells; // nodeId, o -1 si esta libre
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

        // #region Lecturas

        public int GetGridH() => _gridH;
        public int GetGridW() => _gridW;
        public List<GridElement> GetElements() => new List<GridElement>(_elements);

        /// <summary>
        /// Si la celda existe en esta rejilla.
        /// </summary>
        public bool IsInside(GridPos pos) =>
            pos.Row >= 0 && pos.Col >= 0 && pos.Row < _gridH && pos.Col < _gridW;

        /// <summary>
        /// El nodeId de la celda, o -1 si esta libre o fuera de la rejilla.
        /// </summary>
        public int GetCellAt(GridPos pos) => IsInside(pos) ? _cells[pos.Row, pos.Col] : -1;

        /// <summary>
        /// El GridElement colocado en la celda, o null si esta libre o fuera de
        /// la rejilla.
        /// </summary>
        public GridElement GetElementAt(GridPos pos)
        {
            int nodeId = GetCellAt(pos);
            if (nodeId == -1) return null;

            return GetElementOf(nodeId);
        }

        /// <summary>
        /// El GridElement de un nodo por su id, o null si el nodo no esta en esta
        /// rejilla. Util cuando ya se conoce el nodo y lo que falta es donde esta:
        /// el sentido contrario de GetElementAt.
        /// </summary>
        public GridElement GetElementOf(int nodeId)
        {
            foreach (GridElement elem in _elements)
                if (elem.GetNode().GetNodeId() == nodeId) return elem;

            return null;
        }

        // #endregion

        // #region Colocacion

        /// <summary>
        /// Si un item de esas dimensiones se puede colocar en la posicion indicada.
        /// </summary>
        /// <param name="ignoreNodeId">
        /// Nodo cuyas celdas cuentan como libres. Hace falta para mover un nodo a una posicion que
        /// se solapa con la suya: la mano guarda una referencia y nada sale de la rejilla hasta
        /// colocarlo, asi que sus celdas siguen con su id y se bloquearia contra si mismo. Bajar
        /// una fila una hoja de 1x3 es el caso tipico.
        /// -1 no bloquea nada de mas, porque NodeIdGenerator empieza en 1.
        /// </param>
        public bool CanPlace(GridPos pos, int itemH, int itemW, int ignoreNodeId = -1)
            => CanPlace(pos, itemH, itemW, ignoreNodeId, -1);

        /// <param name="alsoIgnoreNodeId">
        /// Segundo nodo cuyas celdas cuentan como libres. Un intercambio saca de la rejilla LOS DOS
        /// nodos antes de volver a ponerlos, asi que ninguno puede estorbar al otro, y un solo
        /// id ignorado no puede expresarlo.
        /// </param>
        public bool CanPlace(GridPos pos, int itemH, int itemW, int ignoreNodeId, int alsoIgnoreNodeId)
        {
            if (pos.Row < 0 || pos.Col < 0) return false;
            if (pos.Row + itemH > _gridH || pos.Col + itemW > _gridW) return false;

            for (int r = pos.Row; r < pos.Row + itemH; r++)
                for (int c = pos.Col; c < pos.Col + itemW; c++)
                {
                    int owner = _cells[r, c];
                    if (owner != -1 && owner != ignoreNodeId && owner != alsoIgnoreNodeId) return false;
                }

            return true;
        }

        /// <summary>
        /// Intercambia las celdas de dos nodos ya colocados en esta rejilla.
        ///
        /// No mueve unidades: los dos nodos siguen siendo los mismos y el peso del inventario
        /// no cambia, asi que aqui no hay nada que recomprobar ni sobrantes que devolver. Se
        /// sacan los dos ANTES de colocar para que ninguno estorbe al otro, y si el segundo
        /// Place falla los dos vuelven a sus celdas de partida.
        /// </summary>
        /// <param name="posForA">
        /// Celda donde ira la esquina superior izquierda de A. Se recibe en vez de deducirse de
        /// donde esta B porque A es el que va en la mano y su colocacion la decide el jugador,
        /// celda a celda, igual que en cualquier otra colocacion. B ocupa la esquina que deja A:
        /// el nodo que no esta en la mano no elige por donde se pone.
        /// </param>
        /// <returns>False si alguno no esta en la rejilla, o si no caben intercambiados.</returns>
        public bool SwapNodes(IInventoryElement a, GridPos posForA, IInventoryElement b)
        {
            GridElement elemA = GetElementOf(a.GetNodeId());
            GridElement elemB = GetElementOf(b.GetNodeId());
            if (elemA == null || elemB == null) return false;

            GridPos posA = elemA.GetPos();
            GridPos posB = elemB.GetPos();

            Remove(a.GetNodeId());
            Remove(b.GetNodeId());

            if (Place(a, posForA) && Place(b, posA)) return true;

            Remove(a.GetNodeId());
            Remove(b.GetNodeId());
            Place(a, posA);
            Place(b, posB);

            return false;
        }

        public bool CanPlace(ItemEntity item)
        {
            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            return !FindFirstFit(baseInfo.DimensionH, baseInfo.DimensionW).IsNone;
        }

        /// <summary>
        /// Coloca un nodo en la posicion indicada. Devuelve false si el sitio no esta libre.
        /// Un nodo puede solaparse con sus propias celdas anteriores, asi que esto sirve tambien de
        /// "mover aqui": primero se liberan las celdas viejas; si no, seguirian marcadas con su id y
        /// el nodo ocuparia mas de lo que ocupa.
        /// </summary>
        /// <param name="ignoreNodeId">
        /// Nodo extra cuyas celdas cuentan como libres, ademas de las del propio nodo (ver CanPlace).
        /// Hace falta cuando el movimiento se hace creando un nodo NUEVO y quitando el viejo: el
        /// id nuevo aun no tiene celdas, asi que sin esto la colocacion choca con el nodo al
        /// que sustituye.
        /// </param>
        public bool Place(IInventoryElement node, GridPos pos, int ignoreNodeId = -1)
        {
            BaseItemComponent baseItem = node.GetItemEntity().GetComponent<BaseItemComponent>();
            int itemH = baseItem.DimensionH;
            int itemW = baseItem.DimensionW;

            if (!CanPlace(pos, itemH, itemW, node.GetNodeId())
             && !CanPlace(pos, itemH, itemW, ignoreNodeId)) return false;

            int nodeId = node.GetNodeId();
            Remove(nodeId);   // no hace nada si aun no estaba colocado

            for (int r = pos.Row; r < pos.Row + itemH; r++)
                for (int c = pos.Col; c < pos.Col + itemW; c++)
                    _cells[r, c] = nodeId;

            _elements.Add(new GridElement(node, pos));
            return true;
        }

        public bool TryFirstPlace(IInventoryElement node)
        {
            BaseItemComponent baseInfo = node.GetItemEntity().GetComponent<BaseItemComponent>();
            GridPos pos = FindFirstFit(baseInfo.DimensionH, baseInfo.DimensionW);
            return !pos.IsNone && Place(node, pos);
        }

        /// <summary>
        /// Quita un nodo de la rejilla por su nodeId.
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

        // #endregion

        // #region Consultas

        /// <summary>
        /// Primera posicion donde cabe un item de esas dimensiones.
        /// Recorre de izquierda a derecha y de arriba abajo.
        /// Devuelve la celda, o GridPos.None si no hay sitio.
        /// </summary>
        /// <param name="ignoreNodeId">Nodo cuyas celdas cuentan como libres (ver CanPlace).</param>
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
        /// Numero de celdas libres de la rejilla.
        /// </summary>
        public int GetFreeCellCount()
        {
            int free = 0;
            for (int r = 0; r < _gridH; r++)
                for (int c = 0; c < _gridW; c++)
                    if (_cells[r, c] == -1) free++;
            return free;
        }

        // #endregion
    }
}
