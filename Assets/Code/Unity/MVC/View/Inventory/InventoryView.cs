using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MVC.View.UI.Inventory;
using ECS.Component.Equipment;
using ECS.Component;
using Unity.Services;

namespace MVC.View
{
    public class InventoryView : IView
    {
        #region Fields

        private UIDocument _uiDocument;
        private VisualTreeAsset _itemTemplate;

        private VisualElement _root;

        private List<VisualElement> _leftTabs,
                                    _leftPanels,
                                    _equipmentSlots;

        private VisualElement _itemGrid;
        private VisualElement _titleBar;
        private VisualElement _equipmentContainer;

        private Label _weightLabel;

        private VisualElement _inspectionStrip,
                              _inspectIcon;
        private Label _inspectName,
                      _inspectDescription,
                      _inspectWeight,
                      _inspectDurability,
                      _inspectSize;

        private bool _isReady = false;
        private bool _isDragging;
        private Vector2 _dragOffset;

        #endregion

        #region Events

        public event Action<int> OnItemClicked;
        public event Action OnCloseClicked;
        public event Action OnReady;

        #endregion

        #region Initialization

        public InventoryView(UIDocument uiDocument, VisualTreeAsset itemTemplate)
        {
            _uiDocument = uiDocument;
            _itemTemplate = itemTemplate;
            _leftTabs = new List<VisualElement>();
            _leftPanels = new List<VisualElement>();
        }

        public void Initialize()
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>("inventory-root");
            _root.RegisterCallback<GeometryChangedEvent>(OnRootReady);
        }

        private void OnRootReady(GeometryChangedEvent e)
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnRootReady);

            // Left tabs
            VisualElement leftTabs = _root.Q<VisualElement>("left-tabs-bar");
            _leftTabs = new List<VisualElement>(leftTabs.Children());

            // Left panels
            VisualElement leftPanels = _root.Q<VisualElement>("left-panel");
            _leftPanels = new List<VisualElement>(leftPanels.Children());

            // Equipment slots
            VisualElement equipmentPanel = _root.Q<VisualElement>("equipment-panel");
            _equipmentSlots = equipmentPanel.Query(className: "equip-slot").ToList();

            // Core elements
            _itemGrid           = _root.Q<VisualElement>("item-grid");
            _titleBar           = _root.Q<VisualElement>("title-bar");
            _weightLabel        = _root.Q<Label>("weight-label");
            _equipmentContainer = _root.Q<VisualElement>("equipment-container");

            // Inspection strip
            _inspectionStrip    = _root.Q<VisualElement>("inspection-strip");
            _inspectIcon        = _root.Q<VisualElement>("inspect-icon");
            _inspectIcon.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                _inspectIcon.style.width = evt.newRect.height;
            });
            _inspectName        = _root.Q<Label>("inspect-name");
            _inspectDescription = _root.Q<Label>("inspect-description");
            _inspectWeight      = _root.Q<Label>("inspect-weight");
            _inspectDurability  = _root.Q<Label>("inspect-durability");
            _inspectSize        = _root.Q<Label>("inspect-size");

            _root.Q<Button>("close-button").clicked += () => OnCloseClicked?.Invoke();
            RegisterDrag();
            _isReady = true;
            
            InitSideBarAndLeftPanels();

            Hide();
            OnReady?.Invoke();
        }

        public void GenerateGrid(int rows, int cols)
        {
            VisualElement grid = new VisualElement();
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

            SetInternallGrid(grid);
        } 

        private void InitSideBarAndLeftPanels()
        {  
            // Assing visibility
            foreach (VisualElement panel in _leftPanels)
            {
                if (panel.name == "equipment-panel") panel.style.display = DisplayStyle.Flex;
                else panel.style.display = DisplayStyle.None; 
            }

            foreach (VisualElement tab in _leftTabs)
            {
                tab.RegisterCallback<ClickEvent>(evt =>
                {
                    string relationClass = GetRelationClass(tab);
                    foreach (VisualElement panel in _leftPanels)
                    {
                        panel.style.display = panel.ClassListContains(relationClass)
                            ? DisplayStyle.Flex
                            : DisplayStyle.None;
                    }
                    Debug.Log("Panel izquerdo cambiado a " + relationClass);
                });
            }
        }

        #endregion

        #region Visibility

        public bool IsReady() => _isReady;
        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            ResetPosition();
        }
        public bool IsVisible() => _root.style.display == DisplayStyle.Flex;

        #endregion

        #region Getters 

        #endregion

        #region Inventory Rendering

        public void RenderItems(List<ItemDisplayData> items)
        {
            _itemGrid.Clear();
            foreach (ItemDisplayData item in items)
            {
                VisualElement card = _itemTemplate.CloneTree();
                card.Q<Label>("item-name").text = item.Name;
                card.Q<Label>("item-amount").text = item.Amount > 1 ? item.Amount.ToString() : "";
                int capturedId = item.Id;
                card.RegisterCallback<ClickEvent>(_ => OnItemClicked?.Invoke(capturedId));
                _itemGrid.Add(card);
            }
        }

        public void SetInternallGrid(VisualElement grid)
        {
            _itemGrid.Clear();
            _itemGrid.Add(grid);
        }

        public void UpdateStats(float currentWeight, float maxWeight)
        {
            _weightLabel.text = $"Peso: {currentWeight:F1}/{maxWeight:F1} kg";
        }

        #endregion

        #region Inspection Strip

        public void UpdateInspection(ItemDisplayData item)
        {
            _inspectName.text = item.Name;
            _inspectDescription.text = item.Description;
            _inspectWeight.text = $"Peso: {item.Weight:F1} kg";
            _inspectDurability.text = $"Durabilidad: {item.Durability}";
            _inspectSize.text = $"Tamaño: {item.DimensionW}x{item.DimensionH}";
        }

        public void ClearInspection()
        {
            _inspectName.text = "";
            _inspectDescription.text = "";
            _inspectWeight.text = "";
            _inspectDurability.text = "";
            _inspectSize.text = "";
        }

        #endregion

        #region Equipment Rendering

        public void UpdateEquipmentSlots(EquipmentComponent equipment)
        {
            foreach (VisualElement viewSlot in _equipmentSlots)
            { 
                EquipmentSlot realSlot = equipment.GetEquipmentSlot(GetEquipmentSlotType(viewSlot));
                if (!realSlot.IsEnabled()) 
                    SetDisabledSlotTexture(viewSlot);
                else if (realSlot.GetEquippedItemCount() == 0) 
                    SetEmptySlotTexture(viewSlot);
                else
                {
                    SetEquipedItemTexture(viewSlot, realSlot.GetTopItem().GetComponent<BaseItemComponent>().GetIconPath());
                    //Y aqyum su gat más items hacer lo del desplegable
                } 
            }
        }
        
        #endregion

        #region Equipment Rendering - Helpers

        private EquipmentSlotType GetEquipmentSlotType(VisualElement element)
        {
            string slotName = element.name.Replace("slot-", "");
            if (Enum.TryParse<EquipmentSlotType>(slotName, true, out var type)) 
                return type;
            throw new InvalidOperationException($"Equipment slot '{element.name}' has no matching EquipmentSlotType");
        }


        private void SetDisabledSlotTexture(VisualElement element)
        {
            string path = "images/slot/" + element.name + ".png";
        }

        private void SetEmptySlotTexture(VisualElement element)
        {
            // TODO
        }

        private void SetEquipedItemTexture(VisualElement element, string iconPath)
        {
            Texture2D tex = TextureCache.Instance.Get(iconPath);
            if (tex != null)
                element.style.backgroundImage = new StyleBackground(tex);    
        }

        #endregion

        #region Helpers
        private string GetRelationClass(VisualElement element)
        {
            foreach (string cls in element.GetClasses())
                if (cls != "left-tab" && cls != "internal-personal-panel") return cls;
            return null;
        }

        #endregion

        #region Drag

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
                _root.layout.x, _root.layout.y);
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

        #endregion
    }
}
