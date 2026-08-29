using System;
using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Component.Equipment;
using ECS.Entity;
using ECS.Systems;
using Inventory;
using Item;
using MVC.View;
using MVC.View.Inventory;
using MVC.View.UI.Inventory;
using Services;
using UnityEngine;
using AC = Utils.ArgumentChecker;

namespace MVC.Presenter.Inventory
{
    public class InventoryPresenter : IPresenter
    {
        private readonly InventoryView _view;
        private readonly ItemCatalogue _itemCatalog;
        private IEntity _entity;
        private bool _pendingOpen = false;
        private InventoryService _service;

        private Dictionary<PanelType, InventoryPanelPresenter> _panelPresenters;
        public InventoryPresenter(InventoryView view, ItemCatalogue itemCatalogue, InventoryService service)
        {
            _view = view;
            _view.OnCloseClicked += OnCloseClicked;
            _view.OnReady += OnViewReady;
            _view.OnSlotLayersRequested += OnSlotLayersRequested;
            _view.OnCatalogItemGrabbed += OnCatalogItemGrabbed; 
            _view.OnCancelRequested += OnCancelRequested;
            _view.OnReleasedOutsideGrid += OnReleasedOutsideGrid;
            _itemCatalog = itemCatalogue;
            _service  = service; 

            view.Initialize();
        }

        private void InitPanelPresenters()
        {
            _panelPresenters  = new Dictionary<PanelType, InventoryPanelPresenter>();
            
            InventoryPanelPresenter playerPresenter = new InventoryPanelPresenter(_view.GetPanel(PanelType.Player), _service);
            InventoryPanelPresenter panelAPresenter = new InventoryPanelPresenter(_view.GetPanel(PanelType.A), _service);
            InventoryPanelPresenter panelBPresenter = new InventoryPanelPresenter(_view.GetPanel(PanelType.B), _service);
            
            _panelPresenters[PanelType.Player] = playerPresenter;
            _panelPresenters[PanelType.A] = panelAPresenter;
            _panelPresenters[PanelType.B] = panelBPresenter;
            
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
            {
                pres.OnHandChanged += HandChanged;
                pres.OnHandStyleUpdate += UpdateHandDisplay;
            }
        }

        public void Open(IEntity entity)
        {
            _entity = entity;
            if (!_view.IsReady())
            {
                _pendingOpen = true;
                return;
            }
            OpenInternal();
        }

        private void OnViewReady()
        {
            if (_pendingOpen && _entity != null)
            {
                _pendingOpen = false;
                OpenInternal();
            }
        }

        private void OpenInternal()
        {

            InitPanelPresenters(); 

            _view.UpdateEquipmentSlots(_entity.GetComponent<EquipmentComponent>());
            
            List<ItemDisplayData> catalogDTO = new List<ItemDisplayData>();
            foreach (ItemEntity item in _itemCatalog.GetAll())
            {
                catalogDTO.Add(DisplayDTOsBuilder.BuildDisplayData(item, 1));
            }
            _view.FillItemCatalog(catalogDTO); 
            _panelPresenters[PanelType.Player].Bind(_entity);
            _view.Show();
        }
 
        public void Close(bool absolute = true) 
        { 
            if (!_service.IsHandCarrying() || absolute)
            {
                _view.Hide();
            }
            
            _service.EmptyHand(); 
            _view.ClearHandBuffer();
        }
        public bool IsOpen() => _view.IsVisible();

        public void Refresh()
        {
            if (_entity == null || !_view.IsVisible()) return;
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
                pres.Refresh();
        }

        

        private void OnCloseClicked() => Close();

        private void OnSlotLayersRequested(EquipmentSlotType type)
        {
            EquipmentSlot slot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(type);
            List<ItemDisplayData> layers = new List<ItemDisplayData>();

            List<ItemEntity> content = slot.Items;
            for (int i = content.Count - 2; i >= 0; i--)
            {
                layers.Add(DisplayDTOsBuilder.BuildDisplayData(content[i], 1));
            }
            _view.RenderSubslots(layers);
        }

        private void OnCatalogItemGrabbed(int typeId, int amount)
        {
            ItemEntity item = _itemCatalog.CreateItem(typeId);
            int grabbed = _service.SpawnIntoHand(item, amount);
            _view.RenderHandBuffer(DisplayDTOsBuilder.BuildDisplayData(item, grabbed), 
                                   _panelPresenters[PanelType.Player]._panelView.GetCellSize());

        }

        private void HandChanged(Vector2 cellSize)
        {
            RefreshHand(cellSize);
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
                pres.RenderInventory();
        }

        private void UpdateHandDisplay(PlacementVerdict verdict, Vector2 cellSize, int width, int height)
            => _view.UpdateHandDisplay(verdict, cellSize, width, height);

        
        private void RefreshHand(Vector2 cellSize)
        {
            if (!_service.IsHandCarrying()) { _view.ClearHandBuffer(); return; }

            ItemEntity item = _service.GetGrabbedItem();
            _view.RenderHandBuffer(DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount()), cellSize);
        }
         
        /// <summary>
        /// Released outside every grid while dragging. For now it just cancels — the units
        /// never left their node, so there is nothing to lose. When dropping to the ground
        /// exists, this is the method that changes, and the gesture keeps feeling the same.
        /// </summary>
        private void OnReleasedOutsideGrid()
        {
            CancelHand();
        }

        /// <summary>
        /// Click outside every drop target. Cancelling costs nothing: the units never left
        /// their node, so dropping the grab restores the previous state by itself.
        /// </summary>
        private void OnCancelRequested()
        {
            CancelHand();
        }

        /// <summary>
        /// Empties the hand and repaints everything that showed it. No-op with an empty hand,
        /// so callers do not need to check first.
        /// </summary>
        private void CancelHand()
        {
            if (!_service.IsHandCarrying()) return;

            _service.EmptyHand();
            HandChanged(Vector2.zero);   // cellSize irrelevante: sin nada en la mano, se limpia
        }

        /// <summary>
        /// Opens a container in a side slot, or closes it if that container is already there.
        /// Binds before showing so the panel never flashes the previous container's grid.
        /// </summary>
        public void ToggleExtraInventory(IEntity entity, PanelType panel)
        {
            AC.CheckNotNull(entity, nameof(entity));

            if (_view.IsSideContentVisible(panel, SidePanelContent.Inventory))
            {
                CloseInventoryPanel(panel);
                return;
            }

            _panelPresenters[panel].Bind(entity);
            _view.ShowSideContent(panel, SidePanelContent.Inventory);
        }

        public void CloseInventoryPanel(PanelType panel)
        {
            _view.ShowSideContent(panel, SidePanelContent.None);
        }
    
        

        

        
        
        
    }
}
