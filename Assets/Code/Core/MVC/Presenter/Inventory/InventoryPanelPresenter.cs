using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using ECS.Systems;
using Inventory;
using MVC.View.Inventory;
using MVC.View.UI.Inventory;
using Services;
using UnityEngine;

namespace MVC.Presenter.Inventory
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
        private bool _grabbedThisGesture;
        private IEntity _entity;

        public event Action<Vector2> OnHandChanged; 
        public event Action<PlacementVerdict, Vector2, int, int> OnHandStyleUpdate;

        public InventoryPanelPresenter(InventoryPanelView view, InventoryService service)
        {
            _panelView = view;
            _service =  service;

            _panelView.OnCellPressed += OnCellPressed;
            _panelView.OnCellReleased += OnCellReleased;
            _panelView.OnPointerMovedOverCell += EvaluateHandContent;

            
        }
        
        public void Bind(IEntity target)
        {
            _entity = target;
            TetrisGridState grid = _entity.GetComponent<InventoryComponent>().Inventory.GetGrid();
            _panelView.GenerateGrid(grid.GetGridH(), grid.GetGridW());
            Refresh();
        }


        public void Refresh() => RenderInventory();



        private void OnCellPressed(int row, int col)
        {
            _grabbedThisGesture = false;
            if (_service.IsHandCarrying()) return;

            GrabAt(row, col);
            _grabbedThisGesture = _service.IsHandCarrying();   // false si la celda estaba vacia
        }

        private void OnCellReleased(int row, int col, bool dragged)
        {
            if (!_service.IsHandCarrying()) return;
            if (_grabbedThisGesture && !dragged) return;   // agarre por clic: sigue en la mano

            PlaceAt(row, col);

            // Soltar el boton cierra el gesto: si el destino no admitio nada, el arrastre se
            // cancela en vez de dejar el item pegado al cursor. Cancelar es gratis porque las
            // unidades nunca salieron de su nodo. Un clic fallido si mantiene la mano: ahi el
            // gesto sigue abierto hasta el siguiente clic.
            if (dragged && _service.IsHandCarrying()) CancelHand();
        }
        
        /// <summary>
        /// El panel no juzga: pregunta al servicio, que responde por el mismo camino que usaria
        /// para colocar de verdad, y sube la respuesta.
        /// </summary>
        private void EvaluateHandContent(int row, int col, Vector2 cellSize)
        {
            if (_entity == null || !_service.IsHandCarrying()) return;

            PlacementVerdict verdict = _service.EvaluatePlacement(_entity, row, col);

            BaseItemComponent baseInfo = _service.GetGrabbedItem().GetComponent<BaseItemComponent>();
            OnHandStyleUpdate?.Invoke(verdict, cellSize, baseInfo.DimensionW, baseInfo.DimensionH);
        }

        private void GrabAt(int row, int col)
        {
            InventoryObject inventory = _entity.GetComponent<InventoryComponent>().Inventory;
            GridElement element = inventory.GetGrid().GetElementAt(row, col);
            if (element == null) return;   // empty cell: nothing to grab

            ItemObject node = element.GetNode();
            int grabbed = _service.GrabFrom(_entity, inventory, node, node.GetAmount());

            // Painted from what was actually grabbed, not from what the block showed: Grab
            // clamps to what the node holds.
            OnHandChanged?.Invoke(_panelView.GetCellSize()); 
        }
        
        private void PlaceAt(int row, int col)
        {
            int left = _service.PlaceAmountFromHand(_entity, row, col);

            OnHandChanged?.Invoke(_panelView.GetCellSize()); 
        }

        /// <summary>
        /// Repaints the tetris grid contents: one block per placed GridElement,
        /// positioned by its (row, col) and sized by the item's dimensions.
        /// </summary>
        public void RenderInventory()
        {   
            if (_entity == null) return;

            TetrisGridState grid = _entity.GetComponent<InventoryComponent>().Inventory.GetGrid();

            List<GridItemDisplayData> items = new List<GridItemDisplayData>();
            foreach (GridElement element in grid.GetElements())
            {
                ItemObject node = element.GetNode();
                items.Add(new GridItemDisplayData
                {
                    Item      = DisplayDTOsBuilder.BuildDisplayData(node.GetItemEntity(), node.GetAmount()),
                    Row       = element.GetRow(),
                    Col       = element.GetCol(),
                    IsGrabbed = ReferenceEquals(node, _service.GetGrabbedNode())
                });
            }

            _panelView.RenderGridItems(items);
            //TODO gestionar (algun dia que apetezca) la inspection strip
            UpdateWeightStats();
        }

        private void UpdateWeightStats()
        {
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            if (invComp == null) return;

            // No se exige BodyComponent: un arcon no tiene cuerpo pero si limite de peso.
            float currentWeight = invComp.Inventory.GetTotalWeight();
            float maxWeight = CarryCapacity.GetMaxLoad(_entity);
            _panelView.UpdateWeightStats(currentWeight, maxWeight, CarryCapacity.ClassifyLoad(maxWeight > 0 ? currentWeight / maxWeight : 1f));
        } 

        private void CancelHand()
        {
            _service.EmptyHand();
            OnHandChanged?.Invoke(_panelView.GetCellSize());
        }

    }
}