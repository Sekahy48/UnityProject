using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MVC.View.UI.Inventory;
using ECS.Component.Equipment;
using ECS.Component;
using Unity.Services;
using AC = Utils.ArgumentChecker;
using Events;

namespace MVC.View
{
    public class InventoryView : IView
    {
        #region Fields

        private UIDocument _uiDocument;

        private VisualElement _root;

        private List<VisualElement> _leftTabs,
                                    _leftPanels,
                                    _equipmentSlots;

        private VisualElement _subSlotsPopUp,
                              _openPopupSlot;
        private VisualElement _gridMount, _itemsLayer;
        private VisualElement _titleBar;
        private VisualElement _equipmentContainer;

        /// <summary>Hueco lateral izquierdo. Hoy aloja el catalogo de desarrollo.</summary>
        private VisualElement _sidePanelA;

        /// <summary>Boton de desarrollo que abre/cierra el catalogo de items.</summary>
        private Button _devCatalogButton;
        private VisualElement _catalogScroll;

        private VisualElement _weightBar;
        private Label _weightLabel;

        private VisualElement _inspectionStrip,
                              _inspectIcon;
        private VisualElement _tooltip;
        private Label _inspectName,
                      _inspectDescription,
                      _inspectWeight,
                      _inspectDurability,
                      _inspectSize;

        private bool _isReady = false;
        private bool _isDragging;
        private Vector2 _dragOffset;

        private static readonly Dictionary<GameEventType, string> LoadClasses = new()
        {
            { GameEventType.NormalWeight, "load-normal"   },
            { GameEventType.ExtraWeight,  "load-extra"    },
            { GameEventType.Overweight,   "load-over"     },
            { GameEventType.Immobile,     "load-immobile" },
        };
        #endregion

        #region Events

        public event Action OnCloseClicked;
        public event Action OnReady;
        public event Action<EquipmentSlotType> OnSlotLayersRequested;
        public event Action<int, int> OnCatalogItemGrabbed;

        #endregion

        #region Initialization

        public InventoryView(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
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
            _root.RegisterCallback<ClickEvent>(_ => CloseSubslotsPopup());

            // Left tabs
            VisualElement leftTabs = _root.Q<VisualElement>("left-tabs-bar");
            _leftTabs = new List<VisualElement>(leftTabs.Children());

            // Left panels
            VisualElement leftPanels = _root.Q<VisualElement>("left-panel");
            _leftPanels = new List<VisualElement>(leftPanels.Children());

            // Equipment slots
            VisualElement equipmentPanel = _root.Q<VisualElement>("equipment-panel");
            _equipmentSlots = equipmentPanel.Query(className: "equip-slot").ToList();
            AddSubslotsButtons(_equipmentSlots);
            
            _subSlotsPopUp = _root.Q<VisualElement>("subslots-popup");
            _subSlotsPopUp.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            // Core elements
            _gridMount          = _root.Q<VisualElement>("grid-mount");
            _titleBar           = _root.Q<VisualElement>("title-bar");
            _weightLabel        = _root.Q<Label>("weight-label");
            _weightBar = _root.Q<VisualElement>("weight-bar");
            _equipmentContainer = _root.Q<VisualElement>("equipment-container");

            _devCatalogButton = _root.Q<Button>("dev-catalog-button");
            _devCatalogButton.clicked += SwitchItemCatalog;
            _catalogScroll = _uiDocument.rootVisualElement.Q<VisualElement>("catalog-scroll");

            // Los huecos laterales son HERMANOS de inventory-root, no descendientes:
            // hay que buscarlos desde la raiz del documento, no desde _root.
            _sidePanelA = _uiDocument.rootVisualElement.Q<VisualElement>("side-panel-a");
            
            // Inspection strip
            _inspectionStrip    = _root.Q<VisualElement>("inspection-strip");
            _inspectIcon        = _root.Q<VisualElement>("inspect-icon");
            MakeSquare(_inspectIcon);
            _inspectName        = _root.Q<Label>("inspect-name");
            _inspectDescription = _root.Q<Label>("inspect-description");
            _inspectWeight      = _root.Q<Label>("inspect-weight");
            _inspectDurability  = _root.Q<Label>("inspect-durability");
            _inspectSize        = _root.Q<Label>("inspect-size");

            // Tooltip
            _tooltip = _uiDocument.rootVisualElement.Q<VisualElement>("tooltip");
            //
            _root.Q<Button>("close-button").clicked += () => OnCloseClicked?.Invoke();

            // Ventanas fijas: el inventario ocupa un hueco del layout, no flota.
            // El drag se conserva sin usar por si vuelven las ventanas movibles.
            // RegisterDrag();

            _isReady = true;
            
            InitSideBarAndLeftPanels(); 

            Hide();
            OnReady?.Invoke();
        }

        private void AddSubslotsButtons(List<VisualElement> slots)
        {
            foreach(VisualElement slot in slots)
            {
                VisualElement subslotsButton = new VisualElement();
                subslotsButton.AddToClassList("subslots-button");

                subslotsButton.RegisterCallback<ClickEvent>(evt =>
                {
                    if (_openPopupSlot == slot)          // mismo slot y abierto -> cerrar
                    {
                        CloseSubslotsPopup();
                    }
                    else                                  // cerrado, o abierto en otro slot -> abrir aquí
                    {
                        _openPopupSlot = slot;
                        PositionAndShowPopup(slot);
                        OnSlotLayersRequested?.Invoke(GetEquipmentSlotType(slot));
                    }
                    evt.StopPropagation();
                }); 
                
                slot.Add(subslotsButton);
            }
        }

        private void PositionAndShowPopup(VisualElement slot)
        {
            Rect slotRect = slot.worldBound;

            // Esquina superior-derecha del slot, en coordenadas de panel:
            // el popup se despliega hacia la derecha, alineado en altura con el slot.
            Vector2 worldAnchor = new Vector2(slotRect.xMax, slotRect.yMin);

            // Traducida al sistema de coordenadas del padre del popup (main-area)
            Vector2 localAnchor = _subSlotsPopUp.parent.WorldToLocal(worldAnchor);

            _subSlotsPopUp.style.left = localAnchor.x + 4;   // hueco de 4px
            _subSlotsPopUp.style.top  = localAnchor.y;
            _subSlotsPopUp.style.display = DisplayStyle.Flex;
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

            _itemsLayer = new VisualElement();
            _itemsLayer.AddToClassList("items-layer");
            grid.Add(_itemsLayer);

            MountGrid(grid); 
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
            _sidePanelA.style.display = DisplayStyle.None;
            ResetPosition();
        }
        public bool IsVisible() => _root.style.display == DisplayStyle.Flex;

        #endregion

        #region Getters 

        #endregion

        #region Inventory Rendering

        public void RenderGridItems(List<GridItemDisplayData> items, int gridH, int gridW)
        {
            _itemsLayer.Clear();
            foreach (GridItemDisplayData item in items)
            {
                Length offsetHPct, offsetVPct, heightPct, widthPct;

                offsetHPct = Length.Percent(item.Col * 100f / gridW);
                offsetVPct = Length.Percent(item.Row * 100f / gridH);
                widthPct = Length.Percent(item.Item.DimensionW * 100f / gridW);
                heightPct = Length.Percent(item.Item.DimensionH * 100f / gridH);

                VisualElement itemCard = new VisualElement();
                VisualElement itemBackground = new VisualElement();
                itemBackground.AddToClassList("item-icon");

                SetBackgroundTexture(itemBackground, item.Item.IconPath);

                itemCard.style.top = offsetVPct;
                itemCard.style.left = offsetHPct;
                itemCard.style.height = heightPct;
                itemCard.style.width = widthPct;
                
                itemCard.AddToClassList("item-block"); 
                itemCard.Add(itemBackground);
                AddAmountLabel(itemCard, item.Item.Amount);

                _itemsLayer.Add(itemCard); 

            }
        }

        private void MountGrid(VisualElement grid)
        {
            _gridMount.Clear();
            _gridMount.Add(grid);
        }

        public void UpdateStats(float currentWeight, float maxWeight, GameEventType eventType)
        {
            _weightLabel.text = $"{currentWeight:F1}/{maxWeight:F1} kg";

            float ratio = maxWeight > 0 ? currentWeight / maxWeight : 1f;
            float painted = Math.Min(ratio, 1f) * 100f;
            _weightBar.style.width = Length.Percent(painted); 

            foreach (string cls in LoadClasses.Values)
                _weightBar.RemoveFromClassList(cls);
            _weightBar.AddToClassList(LoadClasses[eventType]);
        }
        #endregion

        #region Dev Item catalog

        private void SwitchItemCatalog()
        {
            bool visible = _sidePanelA.resolvedStyle.display == DisplayStyle.Flex;
            _sidePanelA.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void FillItemCatalog(List<ItemDisplayData> items)
        {
            AC.CheckNotNull(items, nameof(items));
            _catalogScroll.Clear();

            foreach (ItemDisplayData item in items) 
                BuildCatalogRow(item); 
        }

        private void BuildCatalogRow(ItemDisplayData item)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("catalog-row");

            VisualElement rowIcon = new VisualElement();
            SetBackgroundTexture(rowIcon, item.IconPath);
            MakeSquare(rowIcon);

            Label rowLabel = new Label(item.TypeName);
            rowLabel.AddToClassList("alegreyaSansSC");
            rowLabel.AddToClassList("catalog-row-label");
            Label tooltipLabel = new Label(item.TypeName + "\n" + "\n" + item.Description);
            tooltipLabel.AddToClassList("alegreyaSansSC");
            tooltipLabel.AddToClassList("beigeColor");
            tooltipLabel.AddToClassList("tooltip-text");
            RegisterDelayedAction(rowLabel, () => ShowTooltip(rowLabel, tooltipLabel), 500);

            Label dimLabel = new Label($"{item.DimensionH}x{item.DimensionW}");
            dimLabel.AddToClassList("alegreyaSansSC");

            IntegerField amountField = new IntegerField { value = 1 };
            amountField.AddToClassList("row-amount");
            amountField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 1) amountField.SetValueWithoutNotify(1);
            });

            Button addButton = new Button
            {
                text = "+"
            };
            addButton.AddToClassList("add-button");
            addButton.clicked += () => OnCatalogItemGrabbed?.Invoke(item.TypeId, amountField.value);
            MakeSquare(addButton);

            row.Add(rowIcon);
            row.Add(rowLabel);
            row.Add(dimLabel);
            row.Add(amountField);
            row.Add(addButton);

            _catalogScroll.Add(row);
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
                VisualElement popUpButton = viewSlot.Q<VisualElement>(className: "subslots-button");
                popUpButton.style.display = DisplayStyle.None;

                if (!realSlot.IsEnabled()) 
                    SetDisabledSlotTexture(viewSlot);
                else if (realSlot.GetEquippedItemCount() == 0) 
                    SetEmptySlotTexture(viewSlot);
                else
                {
                    SetBackgroundTexture(viewSlot, realSlot.GetTopItem().GetComponent<BaseItemComponent>().GetIconPath());
                    //Guardar referencia al item contenido en el slot o al slot como objeto para permitir operaciones de movimiento de items
                    
                    if (realSlot.GetEquippedItemCount() > 1)
                    {
                        popUpButton.style.display = DisplayStyle.Flex;
                    }  
                } 
            }
        }
 
        public void RenderSubslots(List<ItemDisplayData> layers)
        {
            _subSlotsPopUp.Clear();
            foreach (ItemDisplayData item in layers)
            {
                VisualElement subSlot = new VisualElement();
                subSlot.AddToClassList("equip-slot");
                SetBackgroundTexture(subSlot, item.IconPath);

                _subSlotsPopUp.Add(subSlot);
            }
        } 

        private void CloseSubslotsPopup()
        {
            _subSlotsPopUp.style.display = DisplayStyle.None;
            _openPopupSlot = null;          
        }

        private void UpdateInternalEquipmentSlot(EquipmentSlot realSlot, VisualElement viewSlot)
        {
            List<VisualElement> subSlots = viewSlot.Query<VisualElement>(className: "sub-slot").ToList();
            if (subSlots.Count == 0)
            {
                // Crear tantos como capas pueda tener el slot Real
                // Reobtener la lista
            }
            
            // Setear iconos en los subSlots en base a los items no topLayer del realSlot por orden
            // Y alguna referencia más 
            
        }
        
        #endregion

        #region Tooltip

        private void ShowTooltip(VisualElement origin, VisualElement content)
        {
            _tooltip.Clear();
            _tooltip.Add(content);

            Rect originRect = origin.worldBound;
            Vector2 worldAnchor = new Vector2(originRect.xMax, originRect.yMin);
            Vector2 localAnchor = _tooltip.parent.WorldToLocal(worldAnchor);
            _tooltip.style.left = localAnchor.x;  
            _tooltip.style.top  = localAnchor.y;
            _tooltip.style.display = DisplayStyle.Flex;
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
            string path = "images/slots/" + element.name + "-disabled.png";
            SetBackgroundTexture(element, path);
        }

        private void SetEmptySlotTexture(VisualElement element)
        {
            string path = "images/slots/" + element.name + ".png";
            SetBackgroundTexture(element, path);
        } 

        #endregion

        #region Helpers
        private string GetRelationClass(VisualElement element)
        {
            foreach (string cls in element.GetClasses())
                if (cls != "left-tab" && cls != "internal-personal-panel") return cls;
            return null;
        }

        private void SetBackgroundTexture(VisualElement element, string texturePath)
        {
            Texture2D tex = TextureCache.Instance.Get(texturePath);
            if (tex != null)
                element.style.backgroundImage = new StyleBackground(tex);
            else
                Debug.LogWarning($"No texture found at '{texturePath}'");
        } 

        private void RegisterDelayedAction(VisualElement element, Action action, long millsecs)
        {
            IVisualElementScheduledItem pending = null;

            element.RegisterCallback<PointerEnterEvent>(_ =>
                pending = element.schedule
                                .Execute(() => action?.Invoke())
                                .StartingIn(millsecs));

            element.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                pending?.Pause();
                _tooltip.style.display = DisplayStyle.None;
            });
        }

        /// <summary>
        /// Mantiene el elemento cuadrado copiando su alto al ancho.
        /// USS no tiene aspect-ratio, y el alto no se conoce hasta que el motor
        /// resuelve el layout, asi que hay que esperar a GeometryChangedEvent.
        /// El callback NO se desregistra: el alto puede cambiar (redimensionado de
        /// ventana, escala de UI) y el ancho debe seguirlo.
        /// El alto debe venir de otro sitio: height explicito, o el stretch por
        /// defecto de un contenedor en fila.
        /// </summary>
        private void MakeSquare(VisualElement element)
        {
            element.RegisterCallback<GeometryChangedEvent>(evt =>
                element.style.width = evt.newRect.height);
        }

        private void AddAmountLabel(VisualElement element, int amount)
        {
            Label amountLabel = new Label(amount.ToString());
            amountLabel.AddToClassList("amount-label");
            element.Add(amountLabel);
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
