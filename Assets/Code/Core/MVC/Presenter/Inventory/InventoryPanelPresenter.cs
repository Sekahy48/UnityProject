using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Inventory;
using MVC.View.Inventory;
using Core.MVC.View.UI.Inventory;
using Core.Services;

namespace Core.MVC.Presenter.Inventory
{
    /// <summary>
    /// Drives one inventory grid. Deliberately NOT an IPresenter: it is not registered in
    /// PresenterManager, it binds a target instead of opening for an entity, and its
    /// visibility is decided by whoever owns the slot it sits in.
    /// </summary>
    public class InventoryPanelPresenter
    {   
        public InventoryPanelView _panelView {get;}
        private InventoryService _service;  
        public IEntity Entity {get; private set;}

        /// <summary>Tamaño final del fantasma sobre esta rejilla, y celda como ancla.</summary>
        public event Action<CellSize, CellSize> OnHandChanged;
        /// <summary>Veredicto, tamaño final del fantasma sobre esta rejilla, y celda como ancla.</summary>
        public event Action<PlacementVerdict, CellSize, CellSize> OnHandStyleUpdate;
        public event Action<ItemDisplayData> OnInspectionStripUpdateRequired;
        private readonly GrabGesture _grabGesture;
        public InventoryPanelPresenter(InventoryPanelView view, InventoryService service)
        {
            _panelView = view;
            _service =  service;

            _panelView.OnCellLeftPressed += OnCellLeftPressed;
            _panelView.OnCellReleased += OnCellReleased;
            _panelView.OnCellPortionPressed += OnCellPortionPressed;
            _panelView.OnPointerMovedOverCell += EvaluateHandContent;

            _grabGesture = new GrabGesture(_service);
        }
        
        public void Bind(IEntity target)
        {
            Entity = target;
            TetrisGridState grid = Entity.GetComponent<InventoryComponent>().Inventory.GetGrid();
            _panelView.GenerateGrid(grid.GetGridH(), grid.GetGridW());
            Refresh();
        }


        public void Refresh() => RenderInventory(); 

        private void OnCellLeftPressed(GridPos pos) => _grabGesture.OnPressed(() => GrabAt(pos));

        private void OnCellReleased(GridPos pos, bool dragged)
        {
            _grabGesture.OnReleased(dragged, () => PlaceAt(pos), CancelHand);
            PublishInspection(pos);
        }

        /// <summary>
        /// Gesto con modificador: con la mano vacia toma una porcion de la pila, y con la mano
        /// llena descarga una porcion de lo que lleva. Una sola entrada para los dos sentidos
        /// porque lo que decide cual toca es el estado de la mano, no la tecla.
        /// </summary>
        private void OnCellPortionPressed(GridPos pos, GrabPortion portion)
        {
            bool sameOrigin = _service.IsGrabbedFrom(GetNodeAt(pos));

            _grabGesture.OnPortionPressed(sameOrigin,
                                          () => GrabPortionAt(pos, portion),
                                          () => GrabMoreFromOwnOrigin(portion),
                                          () => PlacePortionAt(pos, portion));

            PublishInspection(pos);
        }

        /// <summary>Suma a la mano otra porcion de lo que queda sin reservar en su propio origen.</summary>
        private void GrabMoreFromOwnOrigin(GrabPortion portion)
        {
            int units = portion.UnitsOf(_service.GetUngrabbedAmount());
            if (units <= 0) return;

            _service.GrabMore(units);

            PublishHandChanged();
        }

        /// <summary>
        /// Datos de inspeccion de un nodo, o null cuando no hay nada que inspeccionar.
        ///
        /// El null no es un caso de error: es "la celda esta vacia", y sube tal cual hasta la
        /// franja para que decida ella que hacer con la ausencia.
        /// </summary>
        private ItemDisplayData DisplayDataOf(IInventoryElement node) => DisplayDTOsBuilder.BuildNodeData(node);

        /// <summary>Anuncia lo que hay en esa celda para la franja de inspeccion.</summary>
        private void PublishInspection(GridPos pos)
            => OnInspectionStripUpdateRequired?.Invoke(DisplayDataOf(GetNodeAt(pos)));
         
        private void EvaluateHandContent(GridPos pos, CellSize cellSize, GrabPortion portion)
        {
            if (Entity == null) return;

            ItemDisplayData focusedItem = null;

            if (_service.IsHandCarrying())
            {
                PlacementVerdict verdict = _service.EvaluatePlacement(Entity, pos,
                                                                      portion.UnitsOf(_service.GetGrabbedAmount()));

                ItemEntity item = _service.GetGrabbedItem();

                OnHandStyleUpdate?.Invoke(verdict, GhostSizeOverGrid(item, cellSize), cellSize);
                focusedItem = DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount());
            } else
            {
                // Respaldo propio de este camino: con el menu contextual abierto el cursor ya
                // no esta sobre la celda, y aun asi la franja debe seguir mostrando ese item.
                IInventoryElement node = GetNodeAt(pos);
                if (node == null && _panelView.LastRightClickedCell != null)
                    node = GetNodeAt(_panelView.LastRightClickedCell.Value);

                focusedItem = DisplayDataOf(node);
            }

            OnInspectionStripUpdateRequired?.Invoke(focusedItem);
        }

        private void GrabAt(GridPos pos)
        {
            InventoryObject inventory = Entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(pos);
            if (element == null) return;   // empty cell: nothing to grab

            IInventoryElement node = element.GetNode();
            _service.GrabFrom(new InventoryNodeOrigin(Entity, inventory, node), node.GetAmount());

            // Painted from what was actually grabbed, not from what the block showed: Grab
            // clamps to what the node holds.
            PublishHandChanged();
        }

        /// <summary>
        /// Agarra parte de la pila que ocupa esa celda. Sin variante: de una pila mixta sale
        /// lo que salga, y quien quiera elegir abre el desglose de sub-lotes.
        /// </summary>
        private void GrabPortionAt(GridPos pos, GrabPortion portion)
        {
            InventoryObject inventory = Entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(pos);
            if (element == null) return;

            IInventoryElement node = element.GetNode();
            int units = portion.UnitsOf(node.GetAmount());
            if (units <= 0) return;

            _service.GrabFrom(new InventoryNodeOrigin(Entity, inventory, node), units);

            PublishHandChanged();
        }

        /// <summary>Descarga parte de la mano en esa celda; el resto se queda agarrado.</summary>
        private void PlacePortionAt(GridPos pos, GrabPortion portion)
        {
            _service.PlaceAmountFromHand(Entity, pos, portion.UnitsOf(_service.GetGrabbedAmount()));

            PublishHandChanged();
        }

        /// <summary>Agarra una variante concreta del nodo que ocupa esa celda.</summary>
        public void GrabVariantAt(GridPos pos, ItemEntity variant, int amount)
        {
            InventoryObject inventory = Entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(pos);
            if (element == null) return;

            IInventoryElement node = element.GetNode();
            _service.GrabFrom(new InventoryNodeOrigin(Entity, inventory, node), amount, variant);

            PublishHandChanged();
        }


        /// <summary>
        /// Descarga la mano en esa celda, o intercambia si el destino no admite nada pero puede
        /// cambiarse de sitio.
        ///
        /// Se pregunta a EvaluatePlacement y no a CanSwapWith directamente, porque el
        /// intercambio es el ULTIMO recurso y solo el veredicto conoce ese orden: apilar sobre
        /// una pila compatible cumple todas las condiciones de un intercambio, asi que
        /// preguntar primero por el switch intercambiaba lo que debia apilarse.
        /// </summary>
        private void PlaceAt(GridPos pos)
        {
            if (_service.EvaluatePlacement(Entity, pos) == PlacementVerdict.Swap)
                _service.SwapFromHand(Entity, pos);
            else
                _service.PlaceAmountFromHand(Entity, pos);

            PublishHandChanged();
        }

        /// <summary>
        /// Tamaño del fantasma sobre ESTA rejilla: celda por dimensiones del item. Lo decide
        /// el destino y no el origen — de donde saliera lo que llevas no dice nada de como
        /// se ve encima de una rejilla.
        /// </summary>
        private CellSize GhostSizeOverGrid(ItemEntity item, CellSize cell)
        {
            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();

            return new CellSize(cell.Width * baseInfo.DimensionW, cell.Height * baseInfo.DimensionH);
        }

        /// <summary>
        /// Anuncia que la mano cambio, ya con el tamaño resuelto contra esta rejilla. Con la
        /// mano vacia va en cero: no hay nada que dimensionar y la vista solo limpia.
        /// </summary>
        private void PublishHandChanged()
        {
            CellSize cell = _panelView.GetCellSize();
            ItemEntity grabbed = _service.GetGrabbedItem();

            OnHandChanged?.Invoke(grabbed == null ? default : GhostSizeOverGrid(grabbed, cell), cell);
        }

        /// <summary>
        /// Repaints the tetris grid contents: one block per placed GridElement,
        /// positioned by its (row, col) and sized by the item's dimensions.
        /// </summary>
        public void RenderInventory()
        {   
            if (Entity == null) return;

            TetrisGridState grid = Entity.GetComponent<InventoryComponent>().Inventory.GetGrid();

            List<GridItemDisplayData> items = new List<GridItemDisplayData>();
            foreach (GridElement element in grid.GetElements())
            {
                IInventoryElement node = element.GetNode();
                ItemEntity item = node.GetItemEntity();

                if (item == null)
                    continue;
                items.Add(new GridItemDisplayData
                {
                    Item      = DisplayDTOsBuilder.BuildNodeData(node),
                    Row       = element.GetRow(),
                    Col       = element.GetCol(),
                    IsGrabbed = node.GetNodeId() == _service.GetGrabbedNodeId()
                });
            }

            _panelView.RenderGridItems(items);
            //TODO gestionar (algun dia que apetezca) la inspection strip
            UpdateWeightStats();
        }

        private void UpdateWeightStats()
        {
            InventoryComponent invComp = Entity.GetComponent<InventoryComponent>();
            if (invComp == null) return;

            // No se exige BodyComponent: un arcon no tiene cuerpo pero si limite de peso.
            float currentWeight = invComp.Inventory.GetTotalWeight();
            float maxWeight = CarryCapacity.GetMaxLoad(Entity);
            _panelView.UpdateWeightStats(currentWeight, maxWeight, CarryCapacity.ClassifyLoad(maxWeight > 0 ? currentWeight / maxWeight : 1f));
        } 

        private void CancelHand()
        {
            _service.EmptyHand();
            PublishHandChanged();
        }

        /// <summary>
        /// Nodo que ocupa una celda, o null si esta libre o cae fuera de la rejilla.
        ///
        /// Devuelve el ItemObject y no su ItemEntity a proposito: el representante solo dice
        /// QUE hay ahi, y quien vaya a actuar sobre ello (equipar, consumir, tirar) necesita
        /// ademas la pila concreta — su nodeId y cuantas unidades tiene. Este presenter es el
        /// unico sitio donde la entidad, su inventario y la rejilla estan juntos, asi que la
        /// traduccion celda -> nodo vive aqui.
        /// </summary>
        /// <summary>
        /// Esquina superior derecha de la card que ocupa esa celda, o el punto cero si la
        /// celda esta libre.
        ///
        /// La celda pulsada puede ser cualquiera de las que ocupa el item, asi que la medida
        /// sale de su celda ORIGEN y de sus dimensiones, no de donde cayo el cursor. Vive aqui
        /// por lo mismo que GetNodeAt: este es el unico sitio donde la entidad, su rejilla y
        /// la vista que la mide estan juntas.
        /// </summary>
        public PanelPoint ItemCornerAt(GridPos pos)
        {
            if (Entity == null) return default;

            GridElement element = Entity.GetComponent<InventoryComponent>()
                                        .Inventory.GetGrid().GetElementAt(pos);
            if (element == null) return default;

            ItemEntity item = element.GetNode().GetItemEntity();
            if (item == null) return default;

            BaseItemComponent info = item.GetComponent<BaseItemComponent>();

            return _panelView.ItemTopRightCorner(new GridPos(element.GetRow(), element.GetCol()),
                                                 info.DimensionW, info.DimensionH);
        }

        public IInventoryElement GetNodeAt(GridPos pos)
        {
            if (Entity == null) return null;

            InventoryObject inventory = Entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(pos);

            return element?.GetNode();
        }
    }
}