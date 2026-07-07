using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using ECS.Systems;
using Inventory;
using MVC.View;
using MVC.View.UI.Inventory;
using UnityEngine;

namespace MVC.Presenter.Inventory
{
    public class InventoryPresenter : IPresenter
    {
        private readonly InventoryView _view;
        private IEntity _entity;
        private int _activeTabIndex = 0;
        private int _pendingTabIndex = -1;

        private List<IInventoryElement> _tabInventories = new List<IInventoryElement>();

        public InventoryPresenter(InventoryView view)
        {
            _view = view;
            _view.OnTabClicked += OnTabClicked;
            _view.OnItemClicked += OnItemClicked;
            _view.OnCloseClicked += OnCloseClicked;
            _view.OnReady += OnViewReady;
            view.Initialize();
        }

        public void Open(IEntity entity, int tabIndex = 1)
        {
            Debug.Log($"[InventoryPresenter] Open - IsReady: {_view.IsReady()}");
            _entity = entity;
            if (!_view.IsReady())
            {
                _pendingTabIndex = tabIndex;
                Debug.Log($"[InventoryPresenter] Guardado pending tab {tabIndex}");
                return;
            }
            OpenInternal(tabIndex);
        }

        private void OnViewReady()
        {
            Debug.Log($"[InventoryPresenter] OnViewReady - pending: {_pendingTabIndex}, entity: {(_entity == null ? "NULL" : "OK")}");
            if (_pendingTabIndex >= 0 && _entity != null)
            {
                int tab = _pendingTabIndex;
                _pendingTabIndex = -1;
                OpenInternal(tab);
            }
        }

        private void OpenInternal(int tabIndex)
        {
            BuildTabs();
            OnTabClicked(tabIndex);
            _view.Show();
        }

        public void Close() => _view.Hide();
        public bool IsOpen() => _view.IsVisible();
        public void NavigateToTab(int tabIndex) => OnTabClicked(tabIndex);
        public int GetActiveTabIndex() => _activeTabIndex;

        public void Refresh()
        {
            if (_entity == null || !_view.IsVisible()) return;
            BuildTabs();
            OnTabClicked(_activeTabIndex);
        }

        private void BuildTabs()
        {
            _tabInventories.Clear();
            List<TabDisplayData> tabs = new List<TabDisplayData>();

            tabs.Add(new TabDisplayData { Index = 0, Label = "EQ", IsEquipment = true });
            tabs.Add(new TabDisplayData { Index = 1, Label = "INV", IsBaseInventory = true });

            _tabInventories.Add(null);
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            _tabInventories.Add(invComp?.Inventory);

            _view.RenderTabs(tabs);
            UpdateStats();
        }

        private void OnTabClicked(int index)
        {
            _activeTabIndex = index;
            _view.SetActiveTab(index);

            if (index == 0)
            {
                _view.ShowEquipmentPanel();
                return;
            }

            IInventoryElement inventory = index < _tabInventories.Count
                ? _tabInventories[index]
                : null;

            if (inventory == null)
            {
                _view.RenderItems(new List<ItemDisplayData>());
                return;
            }

            List<ItemDisplayData> items = new List<ItemDisplayData>();
            _view.ShowInventoryPanel();
            foreach (IInventoryElement elem in inventory.FlattenInventory())
            {
                ItemEntity itemEntity = elem.GetItemEntity();
                BaseItemComponent baseItem = itemEntity
                    .GetComponent<BaseItemComponent>();

                items.Add(new ItemDisplayData
                {
                    Id = itemEntity.GetName(),
                    Name = itemEntity.GetName(),
                    Amount = elem.GetAmount(),
                    IconPath = baseItem?.GetIconPath(),
                    IsContainer = itemEntity.HasComponent(typeof(StorageComponent)),
                    TabIndex = GetTabIndexForContainer(itemEntity)
                });
            }

            _view.RenderItems(items);
            UpdateStats();
        }

        private void OnItemClicked(string itemId)
        {
            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();
            IInventoryElement found = invComp?.Inventory.Find(itemId);
            if (found != null && found.GetItemEntity().HasComponent(typeof(StorageComponent)))
            {
                int tabIndex = GetTabIndexForContainer(found.GetItemEntity());
                if (tabIndex >= 0) OnTabClicked(tabIndex);
            }
        }

        private void OnCloseClicked() => Close();

        private void UpdateStats()
        {
            if (_entity == null) return;

            InventoryComponent invComp = _entity.GetComponent<InventoryComponent>();

            if (invComp == null || !_entity.HasComponent(typeof(BodyComponent))) return;

            float currentWeight = invComp.Inventory.GetTotalWeight();
            float currentVolume = invComp.Inventory.GetTotalVolume();
            float maxWeight = CarryCapacity.GetMaxCarryWeight(_entity);
            float maxVolume = CarryCapacity.GetMaxCarryVolume(_entity);

            _view.UpdateStats(currentWeight, maxWeight, currentVolume, maxVolume);
        }

        private int GetTabIndexForContainer(ItemEntity entity) => -1;
    }
}