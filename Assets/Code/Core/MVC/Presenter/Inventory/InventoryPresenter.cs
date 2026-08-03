using System;
using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Component.Equipment;
using ECS.Entity;
using ECS.Systems;
using Inventory;
using MVC.View;
using MVC.View.UI.Inventory;
using Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.Presenter.Inventory
{
    public class InventoryPresenter : IPresenter
    {
        private readonly InventoryView _view;
        private IEntity _entity;
        private bool _pendingOpen = false;

        public InventoryPresenter(InventoryView view)
        {
            _view = view;
            _view.OnItemClicked += OnItemClicked;
            _view.OnCloseClicked += OnCloseClicked;
            _view.OnReady += OnViewReady;
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
            //RenderInventory();

            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            TetrisGridState grid = invComp.Inventory.GetGrid();
            _view.GenerateGrid(grid.GetGridH(), grid.GetGridW()); 
            _view.Show();
        }

        public void Close() => _view.Hide();
        public bool IsOpen() => _view.IsVisible();

        public void Refresh()
        {
            if (_entity == null || !_view.IsVisible()) return;
            RenderInventory();
        }

        private void RenderInventory()
        {
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            if (invComp == null)
            {
                _view.RenderItems(new List<ItemDisplayData>());
                return;
            }

            List<ItemDisplayData> items = new List<ItemDisplayData>();
            foreach (IInventoryElement elem in invComp.Inventory.FlattenInventory())
            {
                ItemEntity itemEntity = elem.GetItemEntity();
                BaseItemComponent baseItem = itemEntity.GetComponent<BaseItemComponent>();
                items.Add(new ItemDisplayData
                {
                    Id = baseItem.GetTypeId(),
                    Name = itemEntity.GetDisplayName(),
                    Amount = elem.GetAmount(),
                    IconPath = baseItem?.GetIconPath(),
                    Description = baseItem?.GetDescription(),
                    Weight = baseItem != null ? baseItem.GetWeight() : 0f,
                    Durability = baseItem != null ? baseItem.GetDurability() : 0,
                    DimensionW = baseItem != null ? baseItem.GetDimensionW() : 1,
                    DimensionH = baseItem != null ? baseItem.GetDimensionH() : 1,
                    IsContainer = itemEntity.HasComponent(typeof(StorageComponent))
                });
            }

            _view.RenderItems(items);
            _view.ClearInspection();
            UpdateStats();
        }

        private void OnItemClicked(int itemId)
        {
            // TODO: rellenar inspection strip con datos del item
        }

        private void OnCloseClicked() => Close();

        private void UpdateStats()
        {
            if (_entity == null) return;
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            if (invComp == null || !_entity.HasComponent(typeof(BodyComponent))) return;

            float currentWeight = invComp.Inventory.GetTotalWeight();
            float maxWeight = CarryCapacity.GetMaxCarryWeight(_entity);
            _view.UpdateStats(currentWeight, maxWeight);
        } 

        

        
    }
}
