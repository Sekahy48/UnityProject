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
using MVC.View.UI.Inventory;

namespace MVC.Presenter.Inventory
{
    public class InventoryPresenter : IPresenter
    {
        private readonly InventoryView _view;
        private readonly ItemCatalogue _itemCatalog;
        private IEntity _entity;
        private bool _pendingOpen = false;

        public InventoryPresenter(InventoryView view, ItemCatalogue itemCatalogue)
        {
            _view = view;
            _view.OnCloseClicked += OnCloseClicked;
            _view.OnReady += OnViewReady;
            _view.OnSlotLayersRequested += OnSlotLayersRequested;
            _itemCatalog = itemCatalogue;
            view.Initialize();
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
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            TetrisGridState grid = invComp.Inventory.GetGrid();
            _view.GenerateGrid(grid.GetGridH(), grid.GetGridW());

            _view.UpdateEquipmentSlots(_entity.GetComponent<EquipmentComponent>());
            
            List<ItemDisplayData> catalogDTO = new List<ItemDisplayData>();
            foreach (ItemEntity item in _itemCatalog.GetAll())
            {
                catalogDTO.Add(BuildDisplayData(item, 1));
            }
            _view.FillItemCatalog(catalogDTO);
            RenderInventory();

            _view.Show();
        }

        public void Close() => _view.Hide();
        public bool IsOpen() => _view.IsVisible();

        public void Refresh()
        {
            if (_entity == null || !_view.IsVisible()) return;
            RenderInventory();
        }

        /// <summary>
        /// Repaints the tetris grid contents: one block per placed GridElement,
        /// positioned by its (row, col) and sized by the item's dimensions.
        /// </summary>
        private void RenderInventory()
        {
            TetrisGridState grid = _entity.GetComponent<InventoryComponent>().Inventory.GetGrid();

            List<GridItemDisplayData> items = new List<GridItemDisplayData>();
            foreach (GridElement element in grid.GetElements())
            {
                ItemObject node = element.GetNode();
                items.Add(new GridItemDisplayData
                {
                    Item = BuildDisplayData(node.GetItemEntity(), node.GetAmount()),
                    Row  = element.GetRow(),
                    Col  = element.GetCol()
                });
            }

            _view.RenderGridItems(items, grid.GetGridH(), grid.GetGridW());
            _view.ClearInspection();
            UpdateStats();
        }

        private void OnCloseClicked() => Close();

        private void OnSlotLayersRequested(EquipmentSlotType type)
        {
            EquipmentSlot slot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(type);
            List<ItemDisplayData> layers = new List<ItemDisplayData>();

            List<ItemEntity> content = slot.GetItems();
            for (int i = content.Count - 2; i >= 0; i--)
            {
                layers.Add(BuildDisplayData(content[i], 1));
            }
            _view.RenderSubslots(layers);
        }

        private void OnCatalogItemGrabbed(int typeId, int amount)
        {
            // TODO
        }

        private void UpdateStats()
        {
            if (_entity == null) return;
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            if (invComp == null || !_entity.HasComponent(typeof(BodyComponent))) return;

            float currentWeight = invComp.Inventory.GetTotalWeight();
            float maxWeight = CarryCapacity.GetMaxCarryWeight(_entity);
            _view.UpdateStats(currentWeight, maxWeight, CarryCapacity.ClassifyLoad(maxWeight > 0 ? currentWeight / maxWeight : 1f));
        } 

        
        private ItemDisplayData BuildDisplayData(ItemEntity itemEntity, int amount)
        {
            BaseItemComponent baseItem = itemEntity.GetComponent<BaseItemComponent>();
            if (baseItem == null)
                throw new InvalidOperationException(
                    $"Item '{itemEntity.GetDisplayName()}' has no BaseItemComponent");

            return new ItemDisplayData
            {
                TypeId      = baseItem.GetTypeId(),
                Name        = itemEntity.GetDisplayName(),
                TypeName    = itemEntity.GetGenericName(),
                Amount      = amount,
                IconPath    = baseItem.GetIconPath(),
                Description = baseItem.GetDescription(),
                Weight      = baseItem.GetWeight(),
                Durability  = baseItem.GetDurability(),
                DimensionW  = baseItem.GetDimensionW(),
                DimensionH  = baseItem.GetDimensionH(),
                IsContainer = itemEntity.HasComponent(typeof(StorageComponent))
            };
        }
        
    }
}
