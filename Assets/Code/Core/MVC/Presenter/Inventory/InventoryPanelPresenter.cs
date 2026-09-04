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

        public event Action<CellSize> OnHandChanged; 
        /// <summary>Veredicto, tamaño final del fantasma sobre esta rejilla, y celda como ancla.</summary>
        public event Action<PlacementVerdict, CellSize, CellSize> OnHandStyleUpdate;
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

        private void OnCellReleased(GridPos pos, bool dragged) =>
            _grabGesture.OnReleased(dragged, () => PlaceAt(pos), CancelHand);
        
        /// <summary>
        /// El panel no juzga: pregunta al servicio, que responde por el mismo camino que usaria
        /// para colocar de verdad, y sube la respuesta.
        /// </summary>
        private void EvaluateHandContent(GridPos pos, CellSize cellSize)
        {
            if (Entity == null || !_service.IsHandCarrying()) return;

            PlacementVerdict verdict = _service.EvaluatePlacement(Entity, pos);

            // El tamaño lo decide el destino, y aqui el destino es una rejilla: celda por
            // dimensiones. El ancla sigue siendo la celda, para que la esquina del fantasma
            // caiga sobre la celda apuntada y no en medio del item.
            BaseItemComponent baseInfo = _service.GetGrabbedItem().GetComponent<BaseItemComponent>();
            CellSize itemSize = new CellSize(cellSize.Width * baseInfo.DimensionW,
                                             cellSize.Height * baseInfo.DimensionH);

            OnHandStyleUpdate?.Invoke(verdict, itemSize, cellSize);
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
            OnHandChanged?.Invoke(_panelView.GetCellSize()); 
        }
        
        private void PlaceAt(GridPos pos)
        {
            _service.PlaceAmountFromHand(Entity, pos);

            OnHandChanged?.Invoke(_panelView.GetCellSize()); 
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
            OnHandChanged?.Invoke(_panelView.GetCellSize());
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