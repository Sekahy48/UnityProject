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
using Unity.VisualScripting;
using ECS.Entity;
using Services;

namespace MVC.View.Inventory
{
    public class InventoryView : IView
    {
        #region Fields

        /* Structure */
        private UIDocument _uiDocument;

        private VisualTreeAsset _panelTemplate;

        private VisualElement _mainRoot;

        private List<VisualElement> _leftPanels, /*List of interchangeable panels for the left side*/
                                    _leftTabs;   /*List of clickable elements to change between _leftPanels*/
        
        /* Top Bar*/
        private VisualElement       _titleBar;   /*Top bar of the main panel*/
        private Button _devCatalogButton;        /*Button to open the item catalog - DEV TOOL*/
        
        /* Inspection Strip */
        private VisualElement _inspectionStrip, /*Container of all the composing elements of the inspection strip*/
                              _inspectIcon; /*Icon of the inspection strip*/
                              
        private Label _inspectName,
                      _inspectDescription,
                      _inspectWeight,
                      _inspectDurability,
                      _inspectSize;
        
        /* Equipment */
        private List<VisualElement> _equipmentSlots; /*List of equipment slots*/
        private VisualElement _subSlotsPopUp,   /*Pop up where subslots information is shown*/
                              _openedPopupSlot; /*Cached v.e. slot opened which subslots are being shown*/
        
        
        /* Subpanels */ 
        private VisualElement _catalogScroll; /*Scrollable container where items from the catalog are shown*/
        private Dictionary<PanelType, InventoryPanelView> _panels;/* The panel slots at the right of the main content (player inventory) */

        private VisualElement _sidePanelsContainer; /*Panels container*/
        private VisualElement _playerGridContainer, /* V.e where add the grid inventories */
                              _sidePanelAContainer,
                              _sidePanelBContainer;
        
        /* Ventana lateral A completa. Se muestra u oculta entera; su hueco interior es
           _sidePanelAContainer, que es otra cosa. */
        private VisualElement _sidePanelA;

        /* Catalogo de desarrollo. Es OTRO contenido posible del hueco A, no un caso
           especial: la ventana decide que ocupa cada hueco, y nunca son dos cosas. */
        private VisualElement _itemCatalog;

        /* Ventana lateral B completa, contraparte de _sidePanelA. */
        private VisualElement _sidePanelBWindow;
 

        

        /* Overlays */
        private VisualElement _tooltip;
        private VisualElement _handBuffer;
        
        /* State Parameters*/
        private bool _isReady = false; 
        private Vector2 _handAnchorOffset;

        /* Ultima posicion conocida del puntero. La mano se pinta al agarrar, no al moverse,
           asi que sin esto aparece donde la dejo el agarre anterior hasta el primer
           PointerMove: un parpadeo en mitad del panel. */
        private Vector3 _lastPointerPosition;

        /* Que ocupa el hueco A ahora, y que ocupaba antes de que el catalogo se lo pidiera
           prestado. Sin esto, cerrar el catalogo deja el hueco vacio. */
        private SidePanelContent _slotAContent = SidePanelContent.None;
        private SidePanelContent _contentBeforeCatalog = SidePanelContent.None; /* Needed to know where to place the hand buffer while moving */

        /* DEPRECATED */ 
        private bool _isDragging;
        private Vector2 _dragOffset; 

        
        #endregion

        #region Events

        public event Action OnCloseClicked;
        public event Action OnReady;
        public event Action<EquipmentSlotType> OnSlotLayersRequested;
        public event Action<int, int> OnCatalogItemGrabbed;

        

        /// <summary>Click landed outside every drop target. The presenter decides what to cancel.</summary>
        public event Action OnCancelRequested;

        /// <summary>
        /// The pointer was released outside every grid while dragging. Kept apart from
        /// OnCancelRequested because it is a different intent: today it cancels, but this is
        /// where dropping to the ground will hook in.
        /// </summary>
        public event Action OnReleasedOutsideGrid;

        #endregion

        #region Initialization

        public InventoryView(UIDocument uiDocument, VisualTreeAsset panelTemplate)
        {
            _uiDocument = uiDocument;
            _panelTemplate = panelTemplate;
            _leftTabs = new List<VisualElement>();
            _leftPanels = new List<VisualElement>();
        }

        public void Initialize()
        {
            _mainRoot = _uiDocument.rootVisualElement.Q<VisualElement>("inventory-root");
            _mainRoot.RegisterCallback<GeometryChangedEvent>(OnRootReady);
        }

        private void OnRootReady(GeometryChangedEvent e)
        {
            _mainRoot.UnregisterCallback<GeometryChangedEvent>(OnRootReady);
            _mainRoot.RegisterCallback<ClickEvent>(_ => CloseSubslotsPopup()); 
            // Only reports the gesture: clearing the hand is a model operation and belongs to
            // the presenter. Clearing it here would leave the HandBuffer holding units nothing
            // on screen shows any more.
            _uiDocument.rootVisualElement.RegisterCallback<PointerDownEvent>(_ => OnCancelRequested?.Invoke());

            // Solo llegan aqui los up que NO aterrizaron en una rejilla: los paneles cortan
            // la propagacion de los suyos.
            _uiDocument.rootVisualElement.RegisterCallback<PointerUpEvent>(_ => OnReleasedOutsideGrid?.Invoke());

            // Left tabs
            VisualElement leftTabs = _mainRoot.Q<VisualElement>("left-tabs-bar");
            _leftTabs = new List<VisualElement>(leftTabs.Children());

            // Left panels
            VisualElement leftPanel = _mainRoot.Q<VisualElement>("left-panel");
            _leftPanels = new List<VisualElement>(leftPanel.Children());

            // Equipment slots
            VisualElement equipmentPanel = _mainRoot.Q<VisualElement>("equipment-panel");
            _equipmentSlots = equipmentPanel.Query(className: "equip-slot").ToList();
            AddSubslotsButtons(_equipmentSlots);
            
            _subSlotsPopUp = _mainRoot.Q<VisualElement>("subslots-popup");
            _subSlotsPopUp.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            // Core elements 
            _titleBar           = _mainRoot.Q<VisualElement>("title-bar");
            
            _devCatalogButton = _mainRoot.Q<Button>("dev-catalog-button");
            _devCatalogButton.clicked += SwitchItemCatalog;
            _catalogScroll = _uiDocument.rootVisualElement.Q<VisualElement>("catalog-scroll");

            
            // Los huecos laterales son HERMANOS de inventory-root, no descendientes:
            // hay que buscarlos desde la raiz del documento, no desde _mainRoot.
            // Huecos DEDICADOS, no los contenedores que ya alojan otra cosa: la sub-vista
            // de panel hace Clear() sobre su raiz antes de clonar la plantilla, asi que
            // apuntar a "player-panels" borraria el equipo, y a "side-panel-a" el catalogo.
            _sidePanelsContainer = _uiDocument.rootVisualElement.Q<VisualElement>("side-panels");

            _sidePanelA          = _uiDocument.rootVisualElement.Q<VisualElement>("side-panel-a");
            _itemCatalog         = _uiDocument.rootVisualElement.Q<VisualElement>("item-catalog");
            Button closeCatalogButton = _itemCatalog.Q<Button>("catalog-close-button");
            closeCatalogButton.RegisterCallback<ClickEvent>(_ => SwitchItemCatalog());
            _sidePanelAContainer = _uiDocument.rootVisualElement.Q<VisualElement>("side-panel-a-slot");
            _sidePanelBWindow    = _uiDocument.rootVisualElement.Q<VisualElement>("side-panel-b");
            _sidePanelBContainer = _uiDocument.rootVisualElement.Q<VisualElement>("side-panel-b-slot");
            _playerGridContainer = _uiDocument.rootVisualElement.Q<VisualElement>("player-grid-slot");
            
            
            // Inspection strip
            _inspectionStrip    = _mainRoot.Q<VisualElement>("inspection-strip");
            _inspectIcon        = _mainRoot.Q<VisualElement>("inspect-icon");
            MakeSquare(_inspectIcon);
            _inspectName        = _mainRoot.Q<Label>("inspect-name");
            _inspectDescription = _mainRoot.Q<Label>("inspect-description");
            _inspectWeight      = _mainRoot.Q<Label>("inspect-weight");
            _inspectDurability  = _mainRoot.Q<Label>("inspect-durability");
            _inspectSize        = _mainRoot.Q<Label>("inspect-size");

            // Tooltip
            _tooltip = _uiDocument.rootVisualElement.Q<VisualElement>("tooltip");
            // Hand buffer
            _handBuffer = _uiDocument.rootVisualElement.Q<VisualElement>("hand-buffer");
            _handBuffer.style.display = DisplayStyle.None;
            RegisterHandFollowsCursor();

            _mainRoot.Q<Button>("close-button").clicked += () => OnCloseClicked?.Invoke();

            // Ventanas fijas: el inventario ocupa un hueco del layout, no flota.
            // El drag se conserva sin usar por si vuelven las ventanas movibles.
            // RegisterDrag();

            _isReady = true;
            
            InitSideBarAndLeftPanels(); 

            InitGridPanels();        

            Hide();
            OnReady?.Invoke();
        }

        private void InitGridPanels()
        {
            _panels = new Dictionary<PanelType, InventoryPanelView>();
             
            InventoryPanelView panelA = new InventoryPanelView(_sidePanelAContainer, _panelTemplate);
            InventoryPanelView panelB = new InventoryPanelView(_sidePanelBContainer, _panelTemplate);
            InventoryPanelView playerPanel = new InventoryPanelView(_playerGridContainer, _panelTemplate);

            panelA.OnPointerMovedOverGrid += MoveHandToCursor;
            panelB.OnPointerMovedOverGrid += MoveHandToCursor; 
            playerPanel.OnPointerMovedOverGrid += MoveHandToCursor;

            // El panel pide cerrarse; quien lo cierra es el que reparte los huecos.
            panelA.OnCloseRequested += () => ShowSideContent(PanelType.A, SidePanelContent.None);
            panelB.OnCloseRequested += () => ShowSideContent(PanelType.B, SidePanelContent.None);

            
            _panels[PanelType.A]  = panelA;
            _panels[PanelType.B] = panelB;
            _panels[PanelType.Player] = playerPanel;
            playerPanel.HideTopBar();
            
        }

        private void AddSubslotsButtons(List<VisualElement> slots)
        {
            foreach(VisualElement slot in slots)
            {
                VisualElement subslotsButton = new VisualElement();
                subslotsButton.AddToClassList("subslots-button");

                subslotsButton.RegisterCallback<ClickEvent>(evt =>
                {
                    if (_openedPopupSlot == slot)          // mismo slot y abierto -> cerrar
                    {
                        CloseSubslotsPopup();
                    }
                    else                                  // cerrado, o abierto en otro slot -> abrir aquí
                    {
                        _openedPopupSlot = slot;
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
        public void Show() => _mainRoot.style.display = DisplayStyle.Flex;
        public void Hide()
        {
            _mainRoot.style.display = DisplayStyle.None;
            ShowSideContent(PanelType.A, SidePanelContent.None);
            ShowSideContent(PanelType.B, SidePanelContent.None);
            ResetPosition();
        }
        public bool IsVisible() => _mainRoot.style.display == DisplayStyle.Flex;

        /// <summary>
        /// Decides what a side slot holds. A slot never shows two things at once, so this is
        /// the single place that resolves the competition between the dev catalog and an
        /// external container — no content has to know the others exist.
        /// Only slot A can hold the catalog; B is inventory-only.
        /// </summary>
        public void ShowSideContent(PanelType slot, SidePanelContent content)
        {
            if (slot == PanelType.Player) return;   // el panel del jugador no se negocia

            VisualElement window = slot == PanelType.A ? _sidePanelA : _sidePanelBWindow;
            VisualElement grid   = slot == PanelType.A ? _sidePanelAContainer : _sidePanelBContainer;

            window.style.display = content == SidePanelContent.None
                                 ? DisplayStyle.None
                                 : DisplayStyle.Flex;

            grid.style.display = content == SidePanelContent.Inventory
                               ? DisplayStyle.Flex
                               : DisplayStyle.None;

            if (slot == PanelType.A)
            {
                _itemCatalog.style.display = content == SidePanelContent.Catalog
                                           ? DisplayStyle.Flex
                                           : DisplayStyle.None;
                _slotAContent = content;
            }

            RefreshSidePanelsContainer();
        }

        /// <summary>
        /// The column of side bands reserves its width even with both bands hidden, so its
        /// visibility follows theirs: shown while any band is open, gone when none is.
        ///
        /// Reads style and not resolvedStyle on purpose: ShowSideContent has just written the
        /// bands' display and the engine has not resolved the pass yet, so resolvedStyle would
        /// still report the previous state.
        /// </summary>
        private void RefreshSidePanelsContainer()
        {
            bool anyOpen = _sidePanelA.style.display == DisplayStyle.Flex
                        || _sidePanelBWindow.style.display == DisplayStyle.Flex;

            _sidePanelsContainer.style.display = anyOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Whether a side slot is currently showing that content. Its window must be open AND
        /// the content be the one on display: a hidden window with the grid mounted is not
        /// visible, and an open window showing the catalog is not showing an inventory.
        /// </summary>
        public bool IsSideContentVisible(PanelType slot, SidePanelContent content)
        {
            if (slot == PanelType.Player || content == SidePanelContent.None) return false;

            VisualElement window = slot == PanelType.A ? _sidePanelA : _sidePanelBWindow;
            if (window.resolvedStyle.display != DisplayStyle.Flex) return false;

            VisualElement shown = content == SidePanelContent.Catalog
                                ? _itemCatalog
                                : (slot == PanelType.A ? _sidePanelAContainer : _sidePanelBContainer);

            return shown.resolvedStyle.display == DisplayStyle.Flex;
        }

        

        #endregion

        #region Hand Buffer Rendering 
        
        public void RenderHandBuffer(ItemDisplayData itemData, CellSize cellSize)
        {
            _handBuffer.Clear();
            UIElementUtils.SetBackgroundTexture(_handBuffer, itemData.IconPath);
             
            _handBuffer.style.width  = cellSize.Width  * itemData.DimensionW;
            _handBuffer.style.height = cellSize.Height * itemData.DimensionH;

            Label amountLabel = new Label(itemData.Amount.ToString());
            amountLabel.AddToClassList("amount-label");
            _handBuffer.Add(amountLabel);
            
            _handAnchorOffset = new Vector2(cellSize.Width, cellSize.Height) / 2f;
            _handBuffer.style.display = DisplayStyle.Flex;

            // Ya visible: colocarla bajo el cursor antes de que el motor pinte el frame.
            MoveHandToCursor(_lastPointerPosition);
        }

        public void RefreshHandBuffer(int amount)
        {
            _handBuffer.Clear();
            Label amountLabel = new Label(amount.ToString());
            amountLabel.AddToClassList("amount-label");
            _handBuffer.Add(amountLabel);
        }

        public void ClearHandBuffer()
        { 
            _handBuffer.Clear();
            _handBuffer.style.display = DisplayStyle.None;
            _handBuffer.style.backgroundImage = null;
            
        }

        public void UpdateHandDisplay(PlacementVerdict verdict, CellSize cellSize, int wDim, int hDim)
        {
            _handBuffer.RemoveFromClassList("hand-buffer-fits");
            _handBuffer.RemoveFromClassList("hand-buffer-collision");

            switch (verdict)
            {
                case PlacementVerdict.Fits:
                    _handBuffer.AddToClassList("hand-buffer-fits");
                    break;
                case PlacementVerdict.Blocked:
                    _handBuffer.AddToClassList("hand-buffer-collision");
                    break;
                case PlacementVerdict.Outside:
                    // Sin color y sin redimensionar: fuera de una rejilla no hay celda a la que
                    // escalar, y estirar la mano al tamaño de la ultima visitada seria mentira.
                    return;
            }

            if (cellSize.IsZero) return;

            _handAnchorOffset = new Vector2(cellSize.Width, cellSize.Height) / 2f; 
            _handBuffer.style.width  = cellSize.Width  * wDim;
            _handBuffer.style.height = cellSize.Height * hDim;
        }

        #endregion

        #region Dev Item catalog

        /// <summary>
        /// The catalog borrows slot A. Closing it gives the slot back to whatever it was
        /// showing, instead of leaving it empty: opening a chest and peeking at the catalog
        /// should not close the chest.
        /// </summary>
        private void SwitchItemCatalog()
        {
            if (IsSideContentVisible(PanelType.A, SidePanelContent.Catalog))
            {
                ShowSideContent(PanelType.A, _contentBeforeCatalog);
                return;
            }

            _contentBeforeCatalog = _slotAContent;
            ShowSideContent(PanelType.A, SidePanelContent.Catalog);
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
            UIElementUtils.SetBackgroundTexture(rowIcon, item.IconPath);
            MakeSquare(rowIcon);

            Label rowLabel = new Label(item.TypeName);
            rowLabel.AddToClassList("alegreyaSansSC");
            rowLabel.AddToClassList("catalog-row-label");
            Label tooltipLabel = new Label(item.TypeName + "\n\n" + item.Description + "\n\n" + "Peso: " + item.Weight);
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

                if (!realSlot.IsEnabled) 
                    SetDisabledSlotTexture(viewSlot);
                else if (realSlot.GetEquippedItemCount() == 0) 
                    SetEmptySlotTexture(viewSlot);
                else
                {
                    UIElementUtils.SetBackgroundTexture(viewSlot, realSlot.GetTopItem().GetComponent<BaseItemComponent>().IconPath);
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
                UIElementUtils.SetBackgroundTexture(subSlot, item.IconPath);

                _subSlotsPopUp.Add(subSlot);
            }
        } 

        private void CloseSubslotsPopup()
        {
            _subSlotsPopUp.style.display = DisplayStyle.None;
            _openedPopupSlot = null;          
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
            UIElementUtils.SetBackgroundTexture(element, path);
        }

        private void SetEmptySlotTexture(VisualElement element)
        {
            string path = "images/slots/" + element.name + ".png";
            UIElementUtils.SetBackgroundTexture(element, path);
        } 

        #endregion

        #region Helpers
                /// <summary>
        /// Hace que la mano siga al cursor mientras lleva algo.
        ///
        /// <para>El callback va en la raiz del documento, no en el propio elemento: la mano
        /// es PickingMode.Ignore, asi que no recibe eventos de puntero. Y tiene que ser
        /// Ignore — si capturase el puntero estaria bajo el cursor todo el rato y se comeria
        /// los PointerDown de las celdas de la grid, que es justo donde hay que soltar.</para>
        ///
        /// <para>Las coordenadas se convierten con WorldToLocal del PADRE: evt.position es
        /// del panel, mientras que left/top son relativas al contenedor. Hoy coinciden porque
        /// hand-buffer cuelga de la raiz, pero dejarian de hacerlo en cuanto se moviese.</para>
        ///
        /// <para>Con la mano oculta sale de inmediato: PointerMove dispara en cada frame con
        /// movimiento y no hay nada que recolocar.</para>
        /// </summary>
        private void RegisterHandFollowsCursor()
        {
            _handBuffer.pickingMode = PickingMode.Ignore;

            _uiDocument.rootVisualElement.RegisterCallback<PointerMoveEvent>(evt => MoveHandToCursor(evt.position));
        }

        private void MoveHandToCursor(Vector3 panelPosition)
        {
            _lastPointerPosition = panelPosition;
            if (_handBuffer.style.display == DisplayStyle.None) return;

            Vector2 local = _handBuffer.parent.WorldToLocal(panelPosition);
            _handBuffer.style.left = local.x - _handAnchorOffset.x / 2;
            _handBuffer.style.top  = local.y - _handAnchorOffset.y / 2;
        }

        

        private string GetRelationClass(VisualElement element)
        {
            foreach (string cls in element.GetClasses())
                if (cls != "left-tab" && cls != "internal-personal-panel") return cls;
            return null;
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
                _mainRoot.layout.x, _mainRoot.layout.y);
            _titleBar.CapturePointer(e.pointerId);
            e.StopPropagation();
        }

        private void OnDragMove(PointerMoveEvent e)
        {
            if (!_isDragging) return;
            Vector2 newPos = new Vector2(e.position.x, e.position.y) - _dragOffset;
            _mainRoot.style.left = newPos.x;
            _mainRoot.style.top = newPos.y;
        }

        private void OnDragEnd(PointerUpEvent e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _titleBar.ReleasePointer(e.pointerId);
        }

        private void ResetPosition()
        {
            _mainRoot.style.left = StyleKeyword.Null;
            _mainRoot.style.top = StyleKeyword.Null;
        }

        #endregion

        #region Getters

        public InventoryPanelView GetPanel(PanelType type)
        {
            return _panels[type];
        }

        #endregion
    }

    public enum PanelType
    {
        Player,
        A,
        B,
    }

    /// <summary>What a side slot is currently holding.</summary>
    public enum SidePanelContent
    {
        None,
        Catalog,
        Inventory,
    }
}
