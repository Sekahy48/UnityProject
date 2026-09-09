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
        /// Datos de inspeccion de un nodo, o null cuando no hay nada que inspeccionar.
        ///
        /// El null no es un caso de error: es "la celda esta vacia", y sube tal cual hasta la
        /// franja para que decida ella que hacer con la ausencia.
        /// </summary>
        private ItemDisplayData DisplayDataOf(ItemObject node)
        {
            ItemEntity item = node?.GetItemEntity();

            return item == null ? null : DisplayDTOsBuilder.BuildDisplayData(item, node.GetAmount());
        }

        /// <summary>Anuncia lo que hay en esa celda para la franja de inspeccion.</summary>
        private void PublishInspection(GridPos pos)
            => OnInspectionStripUpdateRequired?.Invoke(DisplayDataOf(GetNodeAt(pos)));
         
        private void EvaluateHandContent(GridPos pos, CellSize cellSize)
        {
            if (Entity == null) return;

            ItemDisplayData focusedItem = null;

            if (_service.IsHandCarrying())
            {
                PlacementVerdict verdict = _service.EvaluatePlacement(Entity, pos);

                ItemEntity item = _service.GetGrabbedItem();

                OnHandStyleUpdate?.Invoke(verdict, GhostSizeOverGrid(item, cellSize), cellSize);
                focusedItem = DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount());
            } else
            {
                // Respaldo propio de este camino: con el menu contextual abierto el cursor ya
                // no esta sobre la celda, y aun asi la franja debe seguir mostrando ese item.
                ItemObject node = GetNodeAt(pos);
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

            ItemObject node = element.GetNode();
            _service.GrabFrom(new InventoryNodeOrigin(Entity, inventory, node), node.GetAmount());

            // Painted from what was actually grabbed, not from what the block showed: Grab
            // clamps to what the node holds.
            PublishHandChanged();
        }

        private void PlaceAt(GridPos pos)
        {
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
                ItemObject node = element.GetNode();
                ItemEntity item = node.GetItemEntity();

                if (item == null)
                    continue;
                items.Add(new GridItemDisplayData
                {
                    Item      = DisplayDTOsBuilder.BuildDisplayData(item, node.GetAmount()),
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
        public ItemObject GetNodeAt(GridPos pos)
        {
            if (Entity == null) return null;

            InventoryObject inventory = Entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(pos);

            return element?.GetNode();
        }
    }
}