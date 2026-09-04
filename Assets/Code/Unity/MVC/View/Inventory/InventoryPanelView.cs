
using System;
using System.Collections.Generic; 
using Core.Events;
using Core.Inventory;
using Core.MVC.View.UI.Inventory; 
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View.Inventory
{
    public class InventoryPanelView
    {
        #region Fields
        private readonly  PanelType _panelType;
        private VisualTreeAsset _template;

        /* Hueco donde se clona la plantilla. Toda consulta del arbol sale de aqui, y por eso
           tres paneles pueden repetir los mismos name sin pisarse. */
        public VisualElement RootPanel {get; private set;}

        private VisualElement _topBar;
        private Label _titelLabel; /*Label where the name of the inventory's entity is displayed*/
        private Button _closeButton; /*Button to hide this subpanel*/
        private VisualElement _gridMount, /*Mounting point for the inventory grid*/
                              _grid,      /*Generated grid, sized to fit its mount*/
                              _itemsLayer;/*Layer where items v.e. representation are placed onto*/
        private int _gridW, _gridH;       /*Cached grid dimensions*/

        private VisualElement _weightBar; /*Level bar that show weight capacity and status*/
        private Label _weightLabel; /*Label that shows numeric information about weight capcity and status*/


        /* Suelo del lado de celda: por debajo el icono deja de leerse, y entonces el problema
           es el inventario, no el panel. Sin techo a proposito — la celda aprovecha todo el
           hueco disponible, que es lo que hace util una banda ancha con pocas columnas. */
        private const float MIN_CELL = 24f;

        /* Ultimo lado de celda aplicado. La guarda contra el bucle de layout se hace contra
           esto y no contra resolvedStyle: resolvedStyle no refleja lo que acabamos de escribir
           hasta que el motor resuelve la pasada, asi que no sirve para detectar "ya esta". */
        private float _fittedCell;

        /* Hay un ajuste ya encolado: evita encolar uno por cada GeometryChanged de la misma
           pasada, que es como una banda al abrirse dispara varios seguidos. */
        private bool _fitScheduled;

        /* Ultima celda sobre la que se emitio veredicto. El veredicto solo puede cambiar al
           cambiar de celda, asi que sin esto se reevalua y se reescriben estilos en cada pixel
           de movimiento. MinValue = ninguna, para que la primera siempre emita. */
        private GridPos? _lastCell;

        private Vector3 _pressLeftOrigin; /*Position where last pointer down event ocurred (left button)*/ 
        private const float DRAG_THRESHOLD_SQR = 100f; /*Threshold to consider a pointer down event means a drag action but a click/grab*/

        private static readonly Dictionary<GameEventType, string> LoadClasses = new()
        {
            { GameEventType.NormalWeight, "load-normal"   },
            { GameEventType.ExtraWeight,  "load-extra"    },
            { GameEventType.Overweight,   "load-over"     },
            { GameEventType.Immobile,     "load-immobile" },
        };

        #endregion

        #region Events

        public event Action<GridPos> OnCellLeftPressed;
        public event Action<GridPos, PanelType> OnCellRightPressed;
        public event Action<GridPos, bool> OnCellReleased;

        public event Action<Vector3> OnPointerMovedOverGrid;
        public event Action<GridPos, CellSize> OnPointerMovedOverCell;

        /* El panel no se oculta a si mismo: pide que lo cierren. Quien decide que ocupa
           cada hueco es InventoryView, y la visibilidad va con esa decision. */
        public event Action OnCloseRequested;

        #endregion

        #region Initialization

        /// <param name="rootPanel">Hueco donde se clona la plantilla y vive el contenido.</param>
        public InventoryPanelView(VisualElement rootPanel, VisualTreeAsset template, PanelType type)
        {
            RootPanel = rootPanel;
            _template  = template;
            _panelType = type;
            BuildAndCache();
        }

        public void BuildAndCache()
        {
            RootPanel.Clear();
            _template.CloneTree(RootPanel);

            _gridMount = RootPanel.Q<VisualElement>("grid-mount");

            _topBar = RootPanel.Q<VisualElement>("inventory-bar");
            _titelLabel = RootPanel.Q<Label>("title-bar-panel");
            _closeButton = RootPanel.Q<Button>("close-button-panel");
            _closeButton.RegisterCallback<ClickEvent>(_ => OnCloseRequested?.Invoke());
            _weightLabel = RootPanel.Q<Label>("weight-label");
            _weightBar = RootPanel.Q<VisualElement>("weight-bar");

            // Una sola suscripcion por panel: MountGrid corre en cada Bind, y registrarlo ahi
            // acumulaba un callback por contenedor abierto.
            //
            // El ajuste se DIFIERE con schedule en vez de correr dentro del callback: escribir
            // un tamano durante la pasada de layout obliga a recalcularla, y si el hueco a su
            // vez depende del contenido, la pasada nunca cierra ("recursive layout"). Diferido
            // se ejecuta con el layout ya resuelto, asi que cada ajuste parte de medidas
            // reales y converge en una o dos vueltas.
            _gridMount.RegisterCallback<GeometryChangedEvent>(_ => ScheduleFit());
        }

        #endregion

        #region Building

        public void GenerateGrid(int rows, int cols)
        {
            _gridH = rows;
            _gridW = cols;
            _fittedCell = 0f;   // dimensiones nuevas: el ajuste anterior ya no vale
            _lastCell = null;   // rejilla nueva: la celda recordada ya no existe

            VisualElement grid = new VisualElement();
            _grid = grid;
            grid.AddToClassList("inventory-grid");
            for (int i = 0; i < rows; i++)
            {
                VisualElement row = new VisualElement();
                grid.Add(row);
                row.AddToClassList("inventory-grid-row");
                for (int j = 0; j < cols; j++)
                {
                    VisualElement cell = new VisualElement();
                    cell.AddToClassList("inventory-grid-cell");
                    row.Add(cell);
                }
            }

            _itemsLayer = new VisualElement();
            _itemsLayer.AddToClassList("items-layer");
            // Mientras items-layer tiene capturado el puntero (drag) los PointerMove van a el
            // y no llegan a la raiz, asi que la mano necesita seguirlos tambien desde aqui.
            _itemsLayer.RegisterCallback<PointerMoveEvent>(evt => 
            {
                OnPointerMovedOverGrid?.Invoke(evt.position);

                GridPos pos = PointToCoords(evt.position);

                if (_lastCell.HasValue && _lastCell.Value == pos) return;
                _lastCell = pos;

                OnPointerMovedOverCell?.Invoke(pos, GetCellSize());
            });

            // Salir de la rejilla no genera PointerMove, asi que sin esto la mano se queda
            // pintada de valido flotando fuera. Coordenadas imposibles: el veredicto sale
            // Outside por el mismo camino, sin un segundo evento que mantener en sincronia.
            _itemsLayer.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                _lastCell = null;
                OnPointerMovedOverCell?.Invoke(GridPos.None, GetCellSize());
            });

            // Being a drop target IS stopping propagation: whatever does not stop the event
            // reaches the root and cancels. Item blocks are children, so their clicks bubble
            // through here and need no handlers of their own — and the coordinates always come
            // from the cursor, never from the block that happened to be under it.
            _itemsLayer.RegisterCallback<PointerDownEvent>(evt =>
            {
                Vector3 position = evt.position;
                // Sin CapturePointer a proposito: capturar manda todos los eventos del puntero
                // a ESTE elemento hasta soltarlo, asi que un arrastre que empieza en un panel
                // y termina en otro entregaria el PointerUp al panel de origen, con
                // coordenadas ajenas a la rejilla donde se solto. Sin captura, el up lo recibe
                // el panel que hay bajo el cursor, que es el que debe colocar.
                
                // Nota personal 
                // 0 = izdo
                // 1 = dcho
                // 2 = central
                GridPos gridPos = PointToCoords(position);
 
                if (evt.button == 0)
                { 
                    _pressLeftOrigin = evt.position; 
                    OnCellLeftPressed.Invoke(gridPos); 
                }     
                else if (evt.button == 1)
                {    
                    OnCellRightPressed.Invoke(gridPos, _panelType);  
                }  
            });

            _itemsLayer.RegisterCallback<PointerUpEvent>(evt =>
            {
                // Igual que en el down: soltar sobre una rejilla es un destino valido, y cortar
                // aqui es lo que impide que la raiz lo tome por "soltado fuera".
                evt.StopPropagation();

                bool dragged = (evt.position - _pressLeftOrigin).sqrMagnitude > DRAG_THRESHOLD_SQR;
                OnCellReleased?.Invoke(PointToCoords(evt.position), dragged);
            });



            grid.Add(_itemsLayer);

            MountGrid(grid);
        }

        private void MountGrid(VisualElement grid)
        {
            _gridMount.Clear();
            _gridMount.Add(grid);
            ScheduleFit();
        }

        /// <summary>
        /// Sizes the grid so it fits its mount with square cells. Inventories differ in size
        /// but panels do not, so the cell adapts instead of the panel — no scrolling, and what
        /// you see is what you have. Item blocks are laid out in percentages, so they follow
        /// with no extra work.
        /// </summary>
        /// <summary>
        /// Queues one fit for after the current layout pass. Repeated calls within the same
        /// pass collapse into a single run, so a burst of GeometryChanged costs one adjustment.
        /// </summary>
        private void ScheduleFit()
        {
            if (_fitScheduled) return;
            _fitScheduled = true;

            _gridMount.schedule.Execute(() =>
            {
                _fitScheduled = false;
                FitGridToMount();
            });
        }

        private void FitGridToMount()
        {
            if (_grid == null || _gridW <= 0 || _gridH <= 0) return;

            float availW = _gridMount.resolvedStyle.width;
            float availH = _gridMount.resolvedStyle.height;
            if (availW <= 0 || availH <= 0) return;   // layout aun sin resolver

            // Pixeles enteros: con decimales, cada pasada puede dar un valor ligeramente
            // distinto y el ajuste nunca converge. Ademas los bordes salen nitidos.
            float cell = Mathf.Floor(Mathf.Max(Mathf.Min(availW / _gridW, availH / _gridH),
                                               MIN_CELL));

            // Escribir el tamano dispara otro GeometryChanged: sin esta salida se cicla.
            if (Mathf.Abs(cell - _fittedCell) < 1f) return;
            _fittedCell = cell;

            _grid.style.width  = cell * _gridW;
            _grid.style.height = cell * _gridH;
        }

        #endregion



        #region Rendering

        public void RenderGridItems(List<GridItemDisplayData> items)
        {
            _itemsLayer.Clear();
            foreach (GridItemDisplayData item in items)
            {
                Length offsetHPct, offsetVPct, heightPct, widthPct;

                offsetHPct = Length.Percent(item.Col * 100f / _gridW);
                offsetVPct = Length.Percent(item.Row * 100f / _gridH);
                widthPct = Length.Percent(item.Item.DimensionW * 100f / _gridW);
                heightPct = Length.Percent(item.Item.DimensionH * 100f / _gridH);

                VisualElement itemCard = new VisualElement();
                VisualElement itemBackground = new VisualElement();
                itemBackground.AddToClassList("item-icon");

                UIElementUtils.SetBackgroundTexture(itemBackground, item.Item.IconPath);

                itemCard.style.top = offsetVPct;
                itemCard.style.left = offsetHPct;
                itemCard.style.height = heightPct;
                itemCard.style.width = widthPct;

                itemCard.AddToClassList("item-block");
                itemCard.Add(itemBackground);
                UIElementUtils.AddAmountLabel(itemCard, item.Item.Amount);

                if (item.IsGrabbed) itemCard.AddToClassList("item-block-grabbed");

                _itemsLayer.Add(itemCard);

            }
        }

        public void UpdateWeightStats(float currentWeight, float maxWeight, GameEventType eventType)
        {
            _weightLabel.text = $"{currentWeight:F1}/{maxWeight:F1} kg";

            float ratio = maxWeight > 0 ? currentWeight / maxWeight : 1f;
            float painted = Math.Min(ratio, 1f) * 100f;
            _weightBar.style.width = Length.Percent(painted);

            foreach (string cls in LoadClasses.Values)
                _weightBar.RemoveFromClassList(cls);
            _weightBar.AddToClassList(LoadClasses[eventType]);
        }

        
        #endregion

        #region Helpers

        /// <summary>
        /// Gets the dimensions in UI absolute measures in base of on screen
        /// elements and the grid number of columns and rows
        /// </summary>
        /// <returns> Dimensions of a cell in the grid </returns>
        public CellSize GetCellSize()
        {
            Vector2 px = CellSizePx();
            return new CellSize(px.x, px.y);
        }

        /// <summary>
        /// La misma medida en el tipo de Unity, para las cuentas internas de esta vista.
        /// Fuera de aqui viaja como CellSize: los presenters viven en Core y no conocen
        /// UnityEngine.
        /// </summary>
        private Vector2 CellSizePx()
        {
            if (_itemsLayer == null || _gridW <= 0 || _gridH <= 0) return Vector2.zero;
            return new Vector2(_itemsLayer.resolvedStyle.width  / _gridW,
                               _itemsLayer.resolvedStyle.height / _gridH);
        }

        /// <summary>
        /// Turns a panel-space pointer position into grid coordinates. Returns values outside
        /// the grid when the cursor is beyond it — checking that is the caller's job, and it is
        /// what tells "over cell (2,3)" from "past the edge".
        /// </summary>
        private GridPos PointToCoords(Vector3 panelPosition)
        {
            Vector2 local = _itemsLayer.WorldToLocal(panelPosition);
            Vector2 cell = CellSizePx();

            if (cell.x <= 0 || cell.y <= 0) return GridPos.None;   // layout aun sin resolver

            return new GridPos(Mathf.FloorToInt(local.y / cell.y),
                               Mathf.FloorToInt(local.x / cell.x));
        }

        /// <summary>
        /// Inverse of PointToCoords: gives the panel-space position of a cell's top-left corner.
        /// Needed by whoever draws over the grid but does not receive pointer positions — the
        /// context menu arrives as a GridPos and has to be placed somewhere.
        /// Returns Vector3.zero for GridPos.None or before the layout resolves; callers that care
        /// about the difference should check IsNone themselves.
        /// </summary>
        public Vector3 CoordsToPoint(GridPos pos)
        {
            if (_itemsLayer == null || pos.IsNone) return Vector3.zero;

            Vector2 cell = CellSizePx();
            if (cell.x <= 0 || cell.y <= 0) return Vector3.zero;   // layout aun sin resolver

            // Col es el eje X y Row el Y, igual que en PointToCoords pero al reves. Si una cambia,
            // la otra tiene que cambiar con ella.
            
            return _itemsLayer.LocalToWorld(new Vector2(pos.Col * cell.x, pos.Row * cell.y) + cell * 0.5f);
        }


        public void SetLabelText(String content)
        {
            _titelLabel.text = content;
        }

        #endregion

        #region  Visibility

        /* Un panel no decide si se ve: eso depende de que ocupa su hueco, y de eso sabe
           InventoryView. Aqui solo queda lo que es del contenido en si. */

        public void HideTopBar() => _topBar.style.display = DisplayStyle.None;
        #endregion
    }
}
