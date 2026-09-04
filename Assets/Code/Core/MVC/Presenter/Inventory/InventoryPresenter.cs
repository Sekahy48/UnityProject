using System;
using System.Collections.Generic;
using Core;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Inventory;
using Core.Item;
using Core.MVC.View;
using MVC.View.Inventory;
using Core.MVC.View.UI.Inventory;
using Core.Services;
using AC = Core.Utils.ArgumentChecker;
using Core.Events;
using Core.Observer;
using Core.ECS.Component.ItemComponents;
using System.Linq;

namespace Core.MVC.Presenter.Inventory
{
    public class InventoryPresenter : IPresenter, IEventObserver
    {    
        private readonly InventoryView _view;
        private readonly ItemCatalogue _itemCatalog;
        private IEntity _entity;
        private bool _pendingOpen = false;
        private InventoryService _service;

        private Dictionary<PanelType, InventoryPanelPresenter> _panelPresenters;
        private readonly GrabGesture _grabGesture;

        public InventoryPresenter(InventoryView view, ItemCatalogue itemCatalogue, InventoryService service)
        {
            _view = view;
            _view.OnCloseClicked += OnCloseClicked;
            _view.OnReady += OnViewReady;
            _view.OnSlotLayersRequested += OnSlotLayersRequested;
            _view.OnCatalogItemGrabbed += OnCatalogItemGrabbed; 
            _view.OnCancelRequested += OnCancelRequested;
            _view.OnReleasedOutsideGrid += OnReleasedOutsideGrid;
            _view.OnEquipmentSlotRightClicked += OnEquipmentSlotRightClicked;
            _view.OnPointerMovedOverSlot += EvaluateHandOverSlot;
            _view.OnPointerLeftSlot += OnPointerLeftSlot;
            _view.OnSubSlotLeftPressed += OnSubSlotLeftPressed;
            _view.OnSubSlotLeftReleased += OnSubSlotLeftReleased;
            _itemCatalog = itemCatalogue;
            _service  = service; 
            _grabGesture = new GrabGesture(service);
            view.Initialize();
        }

        /// <summary>
        /// Crea los presenters de panel y los engancha a sus vistas. Se llama UNA vez por
        /// vida de la vista, no en cada apertura: las InventoryPanelView son siempre las
        /// mismas instancias, asi que reconstruir los presenters dejaria a los anteriores
        /// suscritos a sus eventos. Con dos suscriptores, un clic agarra en el primero y
        /// coloca en el segundo dentro del mismo gesto, y el agarre por clic deja de existir.
        /// </summary>
        private void InitPanelPresenters()
        {
            if (_panelPresenters != null) return;

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
                pres._panelView.OnCellRightPressed += OnCellRightPressed;
            }
        }

        public void Open(IEntity entity)
        {
            _entity = entity; 
            EventBus.GetInstance().Subscribe(GameEventType.InventoryChanged, this);
            EventBus.GetInstance().Subscribe(GameEventType.EquipmentChanged, this);
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

        /// <summary>
        /// Two-stroke close view method. It first empties the hand, then if invoked a second time it hides de view. 
        /// It also hides the view if the parameter absolute is specified as true or not sspecified at all (default value is true).
        /// </summary>
        /// <param name="absolute"></param>
        public void Close(bool absolute = true) 
        { 
            _view.DismissOverlays();
            if (!_service.IsHandCarrying() || absolute)
            {
                _view.Hide();
                EventBus.GetInstance().Unsubscribe(GameEventType.InventoryChanged, this);
                EventBus.GetInstance().Unsubscribe(GameEventType.EquipmentChanged, this);
            }
            
            OnCancelRequested();
        }
        public bool IsOpen() => _view.IsVisible();

        public void Refresh()
        {
            if (_entity == null || !_view.IsVisible()) return;
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
                pres.Refresh();
            _view.CloseContextualMenu();
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

            ItemDisplayData data = DisplayDTOsBuilder.BuildDisplayData(item, grabbed);
            CellSize cellSize = _panelPresenters[PanelType.Player]._panelView.GetCellSize();

            _view.RenderHandBuffer(data, HandGhostSize(data, cellSize), cellSize);
        }

        private void HandChanged(CellSize cellSize)
        {
            RefreshHand(cellSize);
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
                pres.RenderInventory();
        }

        private void UpdateHandDisplay(PlacementVerdict verdict, CellSize itemSize, CellSize anchorBasis)
            => _view.UpdateHandDisplay(verdict, itemSize, anchorBasis);

        /// <summary>
        /// El puntero pasa sobre un slot de equipo llevando algo. Pregunta al servicio por el
        /// mismo camino que usaria para equipar de verdad y sube el veredicto ya traducido.
        ///
        /// Sobre un slot el fantasma se pinta del tamaño del slot, no celda x dimensiones: el
        /// destino manda sobre el tamaño, igual que en la rejilla manda la celda.
        /// </summary>
        private void EvaluateHandOverSlot(int layer, bool subslots, CellSize slotSize)
        {
            if (_entity == null || !_service.IsHandCarrying()) return;

            EquipmentSlotType slotType = CurrentSubSlotType(subslots);
            ItemEntity item = _service.GetGrabbedItem();

            EquipResult result = _service.EvaluateEquip(_entity, slotType, OccupiedSlots(item, slotType));

            _view.UpdateHandDisplay(ToVerdict(result), slotSize, slotSize);
        }

        /// <summary>
        /// El dominio responde en su vocabulario y aqui se traduce al que entiende la vista,
        /// igual que CarryCapacity.ClassifyLoad se traduce a una clase USS. El motivo del
        /// rechazo se pierde a proposito: un color no puede transportarlo.
        /// </summary>
        private static PlacementVerdict ToVerdict(EquipResult result)
            => result == EquipResult.SuccessEquip ? PlacementVerdict.Fits : PlacementVerdict.Blocked;

        /// <summary>Fuera de todo slot: sin color y sin redimensionar.</summary>
        private void OnPointerLeftSlot()
            => _view.UpdateHandDisplay(PlacementVerdict.Outside, default, default);

        
        private void RefreshHand(CellSize cellSize)
        {
            if (!_service.IsHandCarrying()) { _view.ClearHandBuffer(); return; }

            ItemEntity item = _service.GetGrabbedItem();
            ItemDisplayData data = DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount());

            _view.RenderHandBuffer(data, HandGhostSize(data, cellSize), cellSize);
        }

        /// <summary>
        /// Tamaño del fantasma en el instante de agarrar, cuando todavia no hay destino y lo
        /// unico que se sabe es de donde salio. Una rejilla mide en celdas (celda x
        /// dimensiones); un slot mide la prenda entera, asi que su tamaño ya ES el resultado.
        ///
        /// A partir de ahi manda el DESTINO, no el origen: cada PointerMove recalcula el
        /// tamaño contra la rejilla o el slot que haya debajo y llama a UpdateHandDisplay.
        /// Por eso este metodo solo lo usan los dos sitios que pintan el agarre inicial.
        /// </summary>
        private CellSize HandGhostSize(ItemDisplayData data, CellSize cellSize)
        {
            return _service.GetGrabbedOrigin() is EquipmentSlotOrigin
                ? cellSize
                : new CellSize(cellSize.Width * data.DimensionW, cellSize.Height * data.DimensionH);
        }
         

        private void OnSubSlotLeftPressed(int layer, bool subslots)
        {
            _grabGesture.OnPressed(() =>
            {
                EquipmentSlotType slotType = CurrentSubSlotType(subslots);
                EquipmentComponent equipmentComponent = _entity.GetComponent<EquipmentComponent>();
                EquipmentSlot slot = equipmentComponent.GetEquipmentSlot(slotType);
                int realPos = SubslotLayerToRealPos(layer, slot.GetEquippedItemCount());
                if (realPos < 0 || realPos >= slot.GetEquippedItemCount()) return;

                // El origen guarda la prenda, no su capa: el indice se mueve en cuanto
                // alguien equipa o quita algo por encima.
                ItemEntity item = slot.GetItem(realPos);
                _service.GrabFrom(_service.EquipmentOrigin(_entity, OccupiedSlots(item, slotType), item), 1);

                HandChanged(_view.GetEquipmentCellSize());
            });
        }

        private void OnSubSlotLeftReleased(int layer, bool subslots,  bool dragged)
        { 
            _grabGesture.OnReleased(dragged,
            () => 
                {
                    EquipmentSlotType slotType = CurrentSubSlotType(subslots);
                    ItemEntity item = _service.GetGrabbedItem();

                    _service.EquipFromHand(_entity, slotType, OccupiedSlots(item, slotType));
                    HandChanged(_view.GetEquipmentCellSize());
                },
            () =>
                {
                    CancelHand();
                });
        }

        
        /// <summary>
        /// Released outside every grid while dragging. For now it just cancels — the units
        /// never left their node, so there is nothing to lose. When dropping to the ground
        /// exists, this is the method that changes, and the gesture keeps feeling the same.
        /// </summary>
        private void OnReleasedOutsideGrid()
        {
            CoreLogger.Instance.LogWarning("CANCEL por OnReleasedOutsideGrid");
            CancelHand();
    
        }

        /// <summary>
        /// Click outside every drop target. Cancelling costs nothing: the units never left
        /// their node, so dropping the grab restores the previous state by itself.
        /// </summary>
        private void OnCancelRequested()
        {
            CoreLogger.Instance.LogWarning("CANCEL por OnCancelRequested");
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
            HandChanged(default);   // cellSize irrelevante: sin nada en la mano, se limpia
        }

        /// <summary>
        /// Opens a container in a side slot, or closes it if that container is already there.
        /// Binds before showing so the panel never flashes the previous container's grid.
        /// </summary>
        public void ToggleExtraInventory(IEntity entity, PanelType panel)
        {
            AC.CheckNotNull(entity, nameof(entity));
            _view.CloseContextualMenu();

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
    
        private void OnCellRightPressed(GridPos pos, PanelType panel)
        { 
            ItemObject target = _panelPresenters[panel].GetNodeAt(pos);
            IEntity origin = _panelPresenters[panel].Entity;

            if (target == null || target.GetItemEntity() == null) 
                return;
                
            List<ItemAction> actions = _service.GetAvailableActions(target.GetItemEntity(), _entity, origin); 

            RenderContextualMenu(origin, actions, target: target); 
        }

        private void OnEquipmentSlotRightClicked(int layer, bool subslots = false)
        { 
            AC.CheckNotNegative(layer, nameof(layer));
 
            EquipmentSlotType type = CurrentSubSlotType(subslots);
            EquipmentSlot equipmentSlot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(type);

            int realPos = SubslotLayerToRealPos(layer, equipmentSlot.GetEquippedItemCount());
            CoreLogger.Instance.Log(realPos.ToString());
            ItemEntity target = realPos >= 0 ? equipmentSlot.GetItem(realPos) : null;
            if (target == null)
                return;

            List<ItemAction> actions = _service.GetAvailableActions(target, _entity, _entity);

            RenderContextualMenu(_entity, actions, item: target, slotType: type);
        }

        private EquipmentSlotType CurrentSubSlotType(bool subslots)
        {
            return subslots
                ? _view.GetEquipmentSubSlotType(_view.ActiveEquipmentSlot)
                : _view.GetEquipmentSlotType(_view.ActiveEquipmentSlot);
        }

        private int SubslotLayerToRealPos(int layer, int totalLayers) => totalLayers - 1 - layer; 
        private void RenderContextualMenu(IEntity origin, List<ItemAction> actions, ItemObject target = null, ItemEntity item = null, EquipmentSlotType? slotType = null)
        { 
            _view.CloseContextualMenu();

            if (actions.Count == 0)
                return;

            List<MenuOption> options = new List<MenuOption>();
            foreach (ItemAction action in actions)
                options.AddRange(BuildOptions(action, origin, target, item, slotType));

            _view.RenderContextualMenu(options);
        }

        /// <summary>
        /// Traduce una accion posible a las entradas de menu que la representan.
        ///
        /// Devuelve una secuencia y no un MenuOption suelto porque una misma accion puede
        /// dar varias entradas: QuickTransfer se abre en una por cada inventario visible al
        /// que se pueda enviar. Las demas devuelven una sola.
        /// </summary>
        private IEnumerable<MenuOption> BuildOptions(ItemAction action, IEntity origin, ItemObject target = null, ItemEntity item = null, EquipmentSlotType? unequipedSlotType = null) 
        {
            switch (action)
            {
                case ItemAction.DropFromInventory: 
                    return new[] { new MenuOption("Tirar", inputs => OnDropItemRequested(target, origin, inputs.GetInt("amount")), new List<MenuField> { MenuField.Int("amount", max: target.GetAmount()) }) };
                    
                case ItemAction.Equip: 
                    return BuildEquiOptions(target, origin); 

                case ItemAction.Unequip: 
                    return new[] { new MenuOption("Desquipar", inputs => OnUnequipItemRequested(item, origin, unequipedSlotType.Value), new List<MenuField>{})};  

                case ItemAction.Consume:
                    return new[] { new MenuOption("Consumir", inputs => OnConsumeItemRequested(target, origin), new List<MenuField>{}) };

                case ItemAction.QuickTransfer:
                    return BuildTransferOptions(target, origin);

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), $"Sin MenuOption para {action}.");
            }
        }

        private IEnumerable<MenuOption> BuildEquiOptions(ItemObject target, IEntity origin)
        {
            WearableComponent wearableComponent = target.GetItemEntity().GetComponent<WearableComponent>();
            IReadOnlyList<EquipmentSlotType> dstSlotTypes = wearableComponent.TargetSlots;
            if (dstSlotTypes.Count == 0)
                throw new InvalidOperationException("Cannot try to equip an item with no posible slot targets");
            else if (dstSlotTypes.Count ==  1)
                return new[] { new MenuOption("Equipar (" + dstSlotTypes.First().GetDescription() + ")", inputs => OnEquipItemRequested(target, origin, dstSlotTypes.First()), new List<MenuField>{})};
            else if (wearableComponent.FullOcupancy)
            {
                string targetText = "";
                foreach (EquipmentSlotType slotType in dstSlotTypes)
                {
                    targetText += slotType.GetDescription();
                    if (dstSlotTypes.Last() == slotType)
                        continue;
                    else if (dstSlotTypes[dstSlotTypes.Count - 2] == slotType)
                        targetText += " y ";
                    else    
                        targetText += ", ";
                }

                return new[] { new MenuOption("Equipar (" + targetText + ")", inputs => OnEquipItemRequested(target, origin, dstSlotTypes.First()), new List<MenuField>{})};
            }
            else
            {
                List<MenuOption> subOptions = new List<MenuOption>();
                foreach (EquipmentSlotType slotType in dstSlotTypes)
                {
                    subOptions.Add(new MenuOption(slotType.GetDescription(), inputs => OnEquipItemRequested(target, origin, slotType), new List<MenuField>{}));
                }

                return new [] { new MenuOption("Equipar", subOptions)};
            }

        }
        
        /// <summary>
        /// Una entrada por inventario al que se pueda enviar ahora mismo: visible en pantalla,
        /// con entidad enlazada y distinto del de origen. Si no hay ninguno la lista sale vacia
        /// y la accion simplemente no aparece en el menu, sin necesidad de un caso especial.
        ///
        /// La visibilidad se consulta a la View porque es un hecho de presentacion: un arcon
        /// enlazado pero con el panel cerrado no es un destino al que el jugador pueda apuntar.
        /// </summary>
        private IEnumerable<MenuOption> BuildTransferOptions(ItemObject target, IEntity origin)
        {
            List<MenuOption> destinies = new List<MenuOption>();

            foreach (PanelType panel in new[] { PanelType.Player, PanelType.A, PanelType.B })
            {
                IEntity destiny = _panelPresenters[panel].Entity;

                if (destiny == null || destiny == origin) continue;
                if (panel != PanelType.Player &&
                    !_view.IsSideContentVisible(panel, SidePanelContent.Inventory)) continue;

                destinies.Add(new MenuOption(DestinyName(destiny),
                                            inputs => OnQuickTransferRequested(target, origin, destiny, inputs.GetInt("amount")),
                                            new List<MenuField>{ MenuField.Int("amount", target.GetAmount()) }));
            }

            // Sin destinos no hay rama: la entrada no llega a existir, que es justo por lo que
            // este metodo devuelve una secuencia y no un MenuOption suelto.
            if (destinies.Count == 0) return new MenuOption[0];

            return new[] { new MenuOption("Transferir a", destinies) };
        }

        private string DestinyName(IEntity entity)
        {
            NameComponent name = entity.GetComponent<NameComponent>();
            return name != null ? name.DisplayName : "inventario";
        }

        private void OnDropItemRequested(ItemObject target, IEntity origin, int amount, ItemEntity variant = null)
        {
            _service.DropItems(origin, target, amount, variant); 
        }

        private void OnEquipItemRequested(ItemObject target, IEntity origin, EquipmentSlotType dstSlotType)
        {
            WearableComponent wearableComponent = target.GetItemEntity().GetComponent<WearableComponent>();
            IReadOnlyList<EquipmentSlotType> equipmentSlotTypes = wearableComponent.TargetSlots;
            if (!equipmentSlotTypes.Contains(dstSlotType))
                throw new InvalidOperationException("You cannot attempt to unequip an item from a slot where it could never be placed."); 

            InventoryObject srcInventory = origin.GetComponent<InventoryComponent>().Inventory;

            _service.TryEquipItem(new InventoryNodeOrigin(origin, srcInventory, target),
                                  target.GetItemEntity(),
                                  _entity,
                                  OccupiedSlots(target.GetItemEntity(), dstSlotType));
        }

        private void OnUnequipItemRequested(ItemEntity target, IEntity origin, EquipmentSlotType dstSlotType, GridPos? pos = null)
        {
            WearableComponent wearableComponent = target.GetComponent<WearableComponent>();
            IReadOnlyList<EquipmentSlotType> equipmentSlotTypes = wearableComponent.TargetSlots;
            if (!equipmentSlotTypes.Contains(dstSlotType))
                throw new InvalidOperationException("You cannot attempt to unequip an item from a slot where it could never be placed.");
            
            _service.TryUnequipItem(origin, target, OccupiedSlots(target, dstSlotType), pos);
        }

        /// <summary>
        /// Slots que una prenda ocupa realmente: todos los suyos si es de ocupacion completa
        /// (un arco a dos manos), o solo aquel sobre el que se actua.
        /// </summary>
        private static List<EquipmentSlotType> OccupiedSlots(ItemEntity item, EquipmentSlotType slotType)
        {
            WearableComponent wearable = item.GetComponent<WearableComponent>();

            return wearable != null && wearable.FullOcupancy
                ? new List<EquipmentSlotType>(wearable.TargetSlots)
                : new List<EquipmentSlotType> { slotType };
        }

        private void OnConsumeItemRequested(ItemObject target, IEntity origin)
        {
            /*TODO fase2*/
        }

        private void OnQuickTransferRequested(ItemObject target, IEntity origin, IEntity destiny, int amount, ItemEntity variant = null)
        {
            InventoryObject srcInventory = origin.GetComponent<InventoryComponent>().Inventory;

            _service.TryQuickTransfer(new InventoryNodeOrigin(origin, srcInventory, target),
                                      variant, amount, destiny);
        }

        public void UpdateOnEvent(GameEvent gameEvent)
        {
            switch (gameEvent.GetEventType()) 
            {
                case GameEventType.InventoryChanged:
                {
                    IEntity changed = gameEvent.GetEntity();
                    foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
                        if (pres.Entity == changed) { Refresh(); return; }
                    break;
                }

                case GameEventType.EquipmentChanged:
                { 
                    _view.UpdateEquipmentSlots(_entity.GetComponent<EquipmentComponent>());
                    RefreshOpenSubslots();
                    break;
                } 
            }
        }

        /// <summary>
        /// Repinta el popup de capas si esta abierto. Vive aqui y no en el boton porque el
        /// popup es una vista mas del equipo: lo que lo actualiza es que el equipo cambie,
        /// venga el cambio de donde venga.
        /// </summary>
        private void RefreshOpenSubslots()
        {
            if (!_view.IsSubslotsPopupOpen) return;

            EquipmentSlotType type = _view.OpenSubslotsSlotType;
            EquipmentSlot slot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(type);

            // Sin capas por debajo de la superior no hay nada que enseñar, y el boton que lo
            // abre tampoco estaria visible: cerrarlo es lo unico coherente.
            if (slot.GetEquippedItemCount() <= 1) { _view.CloseSubslotsPopup(); return; }

            OnSlotLayersRequested(type);
        }
    }
}
