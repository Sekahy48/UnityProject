using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; 
using MVC.View.UI.Inventory;

namespace MVC.View
{
    public class InventoryView : IView
    {
        private UIDocument _uiDocument;
        private VisualTreeAsset _tabTemplate;
        private VisualTreeAsset _itemTemplate;
        private VisualTreeAsset _equipmentPanelTemplate;

        private VisualElement _root;
        private VisualElement _tabList;
        private VisualElement _itemGrid;
        private VisualElement _titleBar;

        // Campos nuevos
        private VisualElement _contentPanel;
        private VisualElement _statsBar;
        private VisualElement _itemScroll;

        private Label _weightLabel;
        private Label _volumeLabel;

        private bool _isReady = false;

        public event Action<int> OnTabClicked;
        public event Action<string> OnItemClicked;
        public event Action OnCloseClicked;
        public event Action OnReady;

        private bool _isDragging;
        private Vector2 _dragOffset;

        public InventoryView(UIDocument uiDocument, VisualTreeAsset tabTemplate, VisualTreeAsset itemTemplate, VisualTreeAsset equipmentPanelTemplate)
        {
            _uiDocument = uiDocument;
            _tabTemplate = tabTemplate;
            _itemTemplate = itemTemplate;
            _equipmentPanelTemplate = equipmentPanelTemplate;
        }

        public void Initialize()
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>("inventory-root");
            Debug.Log($"[InventoryView] Initialize - _root es {(_root == null ? "NULL" : "OK")}");
            _root.RegisterCallback<GeometryChangedEvent>(OnRootReady);
        }

        private void OnRootReady(GeometryChangedEvent e)
        {
            Debug.Log("[InventoryView] OnRootReady disparado");
            _root.UnregisterCallback<GeometryChangedEvent>(OnRootReady);

            _tabList     = _root.Q<VisualElement>("tab-list");
            _itemGrid    = _root.Q<VisualElement>("item-grid");
            _titleBar    = _root.Q<VisualElement>("title-bar");
            _weightLabel = _root.Q<Label>("weight-label");
            _volumeLabel = _root.Q<Label>("volume-label");
            _contentPanel = _root.Q<VisualElement>("content-panel");
            _statsBar     = _root.Q<VisualElement>("stats-bar");
            _itemScroll   = _root.Q<VisualElement>("item-scroll");

            Debug.Log($"[InventoryView] OnRootReady - _tabList:{(_tabList == null ? "NULL" : "OK")} _titleBar:{(_titleBar == null ? "NULL" : "OK")}");

            _root.Q<Button>("close-button").clicked += () => OnCloseClicked?.Invoke(); 
            RegisterDrag();
            _isReady = true;
            
            Hide();
            OnReady?.Invoke();
        }

        public bool IsReady() => _isReady;
        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() {
            _root.style.display = DisplayStyle.None;
            ResetPosition();
        }
        public bool IsVisible() => _root.style.display == DisplayStyle.Flex;

        public void RenderTabs(List<TabDisplayData> tabs)
        {
            _tabList.Clear();
            foreach (TabDisplayData tab in tabs)
            {
                VisualElement tabElement = _tabTemplate.CloneTree();
                tabElement.Q<Label>("tab-label").text = tab.Label;
                int capturedIndex = tab.Index;
                tabElement.RegisterCallback<ClickEvent>(_ => OnTabClicked?.Invoke(capturedIndex));
                _tabList.Add(tabElement);
            }
        }

        public void SetActiveTab(int index)
        {
            int i = 0;
            foreach (VisualElement tab in _tabList.Children())
            {
                tab.EnableInClassList("tab-item--active", i == index);
                i++;
            }
        }

        public void RenderItems(List<ItemDisplayData> items)
        {
            _itemGrid.Clear();
            foreach (ItemDisplayData item in items)
            {
                VisualElement card = _itemTemplate.CloneTree();
                card.Q<Label>("item-name").text = item.Name;
                card.Q<Label>("item-amount").text = item.Amount > 1 ? item.Amount.ToString() : "";
                string capturedId = item.Id;
                card.RegisterCallback<ClickEvent>(_ => OnItemClicked?.Invoke(capturedId));
                _itemGrid.Add(card);
            }
        }

        public void ShowEquipmentPanel()
        { 
            _itemScroll.style.display = DisplayStyle.None;
            VisualElement existing = _contentPanel.Q<VisualElement>("equipment-panel");
            if (existing == null)
                _contentPanel.Add(_equipmentPanelTemplate.CloneTree());
            else
                existing.style.display = DisplayStyle.Flex;
        }   

        public void ShowInventoryPanel()
        { 
            _itemScroll.style.display = DisplayStyle.Flex;
            VisualElement existing = _contentPanel.Q<VisualElement>("equipment-panel");
            if (existing != null)
                existing.style.display = DisplayStyle.None;
        }
    
        public void UpdateStats(float currentWeight, float maxWeight, float currentVolume, float maxVolume)
        {
            _weightLabel.text = $"Peso: {currentWeight:F1}/{maxWeight:F1} kg";
            _volumeLabel.text = $"Volumen: {currentVolume:F1}/{maxVolume:F1} L";
        }

        private void RegisterDrag()
        {
            _titleBar.RegisterCallback<PointerDownEvent>(OnDragStart);
            _titleBar.RegisterCallback<PointerMoveEvent>(OnDragMove);
            _titleBar.RegisterCallback<PointerUpEvent>(OnDragEnd);
        }

        private void OnDragStart(PointerDownEvent e)
        {
            _isDragging = true;
            _dragOffset = new Vector2(e.position.x, e.position.y) - new Vector2(
                _root.layout.x,
                _root.layout.y);
            _titleBar.CapturePointer(e.pointerId);
            e.StopPropagation();
        }

        private void OnDragMove(PointerMoveEvent e)
        {
            if (!_isDragging) return;
            Vector2 newPos = new Vector2(e.position.x, e.position.y) - _dragOffset;
            _root.style.left = newPos.x;
            _root.style.top = newPos.y;
        }

        private void OnDragEnd(PointerUpEvent e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _titleBar.ReleasePointer(e.pointerId);
        }

        private void ResetPosition()
        {
            _root.style.left = StyleKeyword.Null;
            _root.style.top = StyleKeyword.Null;
        }
    }
}