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
            _view.OnContextualMenuClosed += () => SetPinned(null);
            _view.OnInspectionHovered += SetHovered;
            _view.OnLayerLeftPressed += OnLayerLeftPressed;
            _view.OnLayerLeftReleased += OnLayerLeftReleased;
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
                pres.OnInspectionStripUpdateRequired += UpdateInspectionStrip;
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

            UpdateEquipmentRelated();

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
            _view.RenderLayers(layers);
        }

        private void OnCatalogItemGrabbed(int typeId, int amount)
        {
            ItemEntity item = _itemCatalog.CreateItem(typeId);
            int grabbed = _service.SpawnIntoHand(item, amount);

            ItemDisplayData data = DisplayDTOsBuilder.BuildDisplayData(item, grabbed);

            // Nace sobre la rejilla del jugador, asi que se dimensiona contra ella.
            CellSize cell = _panelPresenters[PanelType.Player]._panelView.GetCellSize();
            CellSize itemSize = new CellSize(cell.Width * data.DimensionW, cell.Height * data.DimensionH);

            _view.RenderHandBuffer(data, itemSize, cell);
        }

        /// <param name="itemSize">Tamaño ya resuelto contra el destino por quien avisa. Cero
        /// cuando la mano queda vacia: no hay nada que dimensionar.</param>
        /// <param name="anchorBasis">Unidad de destino: celda en una rejilla, slot en el equipo.</param>
        private void HandChanged(CellSize itemSize, CellSize anchorBasis)
        {
            RefreshHand(itemSize, anchorBasis);
            foreach (InventoryPanelPresenter pres in _panelPresenters.Values)
            {
                pres.RenderInventory();

                // El peso ya esta reescrito; ahora la nota, que depende de la mano nueva.
                // Solo un panel tiene celda sobrevolada, los demas salen sin hacer nada.
                pres.RepublishHover();
            }

            // La mano es una de las fuentes de la franja, y acaba de cambiar: agarrar o soltar
            // tiene que verse sin esperar a que el cursor se mueva.
            PublishInspection();
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
        private void EvaluateHandOverSlot(int layer, bool fromLayersPopup, CellSize slotSize)
        {
            if (_entity == null) return;

            EquipmentSlotType slotType = CurrentLayerSlotType(fromLayersPopup);

            if (_service.IsHandCarrying())
            {
                ItemEntity item = _service.GetGrabbedItem();

                // Sin excepciones aqui: que una prenda pueda volver al slot del que salio lo
                // resuelve EvaluateEquip descontandola, igual que la rejilla descuenta el nodo
                // que se esta moviendo. Decidirlo en el presenter dejaria a la ejecucion
                // respondiendo otra cosa.
                EquipResult result = _service.EvaluateEquip(_entity, slotType, OccupiedSlots(item, slotType));

                _view.UpdateHandDisplay(ToVerdict(result), slotSize, slotSize);
            }
            

            ItemEntity focusedItem = GarmentAt(slotType, layer);

            SetHovered(focusedItem == null ? null : DisplayDTOsBuilder.BuildDisplayData(focusedItem, 1));
        }

        /// <summary>
        /// La prenda que ocupa una capa de un slot, o null si esa capa no existe.
        /// </summary>
        /// <param name="layer">
        /// Indice tal como lo emite la vista. Un slot de equipo manda siempre 0 y una fila del
        /// popup manda la suya, asi que una sola traduccion sirve para los dos: LayerToRealPos
        /// convierte el 0 en la capa exterior — la que pinta el slot — y el resto en las de
        /// debajo, contando desde fuera hacia dentro.
        /// </param>
        private ItemEntity GarmentAt(EquipmentSlotType slotType, int layer)
        {
            EquipmentSlot slot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(slotType);

            int realPos = LayerToRealPos(layer, slot.GetEquippedItemCount());

            return realPos >= 0 && realPos < slot.GetEquippedItemCount() ? slot.GetItem(realPos) : null;
        }

        /// <summary>
        /// El dominio responde en su vocabulario y aqui se traduce al que entiende la vista,
        /// igual que CarryCapacity.ClassifyLoad se traduce a una clase USS. El motivo del
        /// rechazo se pierde a proposito: un color no puede transportarlo.
        /// </summary>
        private static PlacementVerdict ToVerdict(EquipResult result)
            => result == EquipResult.SuccessEquip ? PlacementVerdict.Fits : PlacementVerdict.Blocked;

        /// <summary>
        /// Fuera de todo slot: sin color, sin redimensionar y sin nada bajo el cursor. Lo
        /// tercero faltaba, y por eso la franja se quedaba con lo ultimo que alguien hubiera
        /// escrito en vez de volver a preguntar.
        /// </summary>
        private void OnPointerLeftSlot()
        {
            _view.UpdateHandDisplay(PlacementVerdict.Outside, default, default);
            SetHovered(null);
        }

        
        /// <summary>
        /// Repinta el fantasma con un tamaño que ya viene resuelto. Este metodo no interpreta
        /// medidas: quien avisa sabe sobre que destino esta y lo calcula alli.
        /// </summary>
        private void RefreshHand(CellSize itemSize, CellSize anchorBasis)
        {
            if (!_service.IsHandCarrying()) { _view.ClearHandBuffer(); return; }

            ItemEntity item = _service.GetGrabbedItem();
            ItemDisplayData data = DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount());

            _view.RenderHandBuffer(data, itemSize, anchorBasis);
        }


        #region Inspection strip

        /* Lo que hay bajo el cursor ahora mismo. Null es una respuesta, no un dato que falta:
           significa "ahi no hay nada que inspeccionar". */
        private ItemDisplayData _hovered;

        /* Lo que fijo el menu contextual abierto, o null si no hay ninguno. Se guarda el
           contexto y no el DTO: FocusedDisplayData se recalcula al leerlo, asi que la franja no
           miente si la pila cambia de cantidad con el menu abierto. */
        private MenuContext? _pinned;

        /// <summary>
        /// Lo que se lleva en la mano, o null con la mano vacia.
        ///
        /// No es una fuente que nadie publique: se PREGUNTA, porque no depende de donde este el
        /// cursor. Antes viajaba disfrazada de "lo que hay debajo" y solo la contaba la rejilla,
        /// asi que en cuanto el cursor salia de una celda la franja se quedaba muda.
        /// </summary>
        private ItemDisplayData HandData()
        {
            ItemEntity item = _service.GetGrabbedItem();

            // Puede ser null con la mano todavia "llena": entre que el origen se vacia y que la
            // mano se entera, RunTransfer ya ha anunciado y alguien puede preguntar. No es un
            // error, es un instante sin nada que enseñar — y por eso se pregunta por el item y
            // no por IsHandCarrying, que responde que si cuando ya no hay nada que pintar.
            return item == null
                ? null
                : DisplayDTOsBuilder.BuildDisplayData(item, _service.GetGrabbedAmount());
        }

        /// <summary>
        /// Unica regla de la franja: manda la mano, si no lo que hay bajo el cursor, y si no el
        /// menu abierto.
        ///
        /// <para>Antes esta prioridad estaba escrita tres veces —el respaldo de la rejilla, el
        /// callback del menu y la nada de los slots— y solo una de las tres era una decision:
        /// las otras dos eran el efecto de que nadie escribiera. Por eso los slots se
        /// comportaban distinto en cuanto algo si escribia (un slot vacio).</para>
        ///
        /// <para>Los paneles informan de lo que tienen debajo; quien decide que se muestra es
        /// esta ventana, que es la dueña de la unica franja que hay.</para>
        /// </summary>
        private void PublishInspection()
            => _view.UpdateInspectionStrip(HandData() ?? _hovered ?? _pinned?.FocusedDisplayData);

        /// <summary>Lo que hay bajo el cursor, o null si no hay nada.</summary>
        private void SetHovered(ItemDisplayData data)
        {
            _hovered = data;
            PublishInspection();
        }

        /// <summary>El menu contextual abierto, o null al cerrarse.</summary>
        private void SetPinned(MenuContext? context)
        {
            _pinned = context;
            PublishInspection();
        }

        #endregion

        private void UpdateInspectionStrip(ItemDisplayData itemData) => SetHovered(itemData);

        private void OnLayerLeftPressed(int layer, bool fromLayersPopup)
        {
            _grabGesture.OnPressed(() =>
            {
                EquipmentSlotType slotType = CurrentLayerSlotType(fromLayersPopup);

                // El origen guarda la prenda, no su capa: el indice se mueve en cuanto
                // alguien equipa o quita algo por encima.
                ItemEntity item = GarmentAt(slotType, layer);
                if (item == null) return;

                _service.GrabFrom(_service.EquipmentOrigin(_entity, OccupiedSlots(item, slotType), item), 1);

                // Sobre un slot la prenda ocupa el slot entero: tamaño y ancla coinciden.
                CellSize slotSize = _view.GetEquipmentCellSize();
                HandChanged(slotSize, slotSize);
            });
        }

        private void OnLayerLeftReleased(int layer, bool fromLayersPopup,  bool dragged)
        { 
            _grabGesture.OnReleased(dragged,
            () => 
                {
                    EquipmentSlotType slotType = CurrentLayerSlotType(fromLayersPopup);
                    ItemEntity item = _service.GetGrabbedItem();

                    
                    if (!_entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(slotType).Items.Contains(item))
                        _service.EquipFromHand(_entity, slotType, OccupiedSlots(item, slotType));
                    else 
                        _service.EmptyHand();

                    CellSize slot = _view.GetEquipmentCellSize();
                    HandChanged(slot, slot);

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
            HandChanged(default, default);   // sin mano no hay nada que dimensionar
        }

        /// <summary>
        /// Opens a container in a side slot, or closes it if THAT SAME container is already
        /// there. Binds before showing so the panel never flashes the previous container's grid.
        ///
        /// <para>Cerrar solo cuando coincide el contenedor: si el hueco muestra otro, pedirlo
        /// aqui significa sustituir, no cerrar. Sin esa distincion, mandar una mochila a un
        /// panel ocupado por un arcon cerraba el arcon y no mostraba nada.</para>
        ///
        /// <para>La comparacion es por identidad y no por Equivalent: dos mochilas iguales y
        /// vacias son equivalentes y no son la misma mochila.</para>
        /// </summary>
        public void ToggleExtraInventory(IEntity entity, PanelType panel)
        {
            AC.CheckNotNull(entity, nameof(entity));
            _view.CloseContextualMenu();

            bool sameContainerShown = _view.IsSideContentVisible(panel, SidePanelContent.Inventory)
                                   && ReferenceEquals(_panelPresenters[panel].Entity, entity);

            if (sameContainerShown)
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
    
        /// <summary>
        /// Con algo en la mano, el clic derecho cancela en vez de abrir menu: el menu actua
        /// sobre lo que hay debajo, y con la mano llena lo que el jugador quiere resolver es la
        /// mano.
        ///
        /// <para>Vive aqui, y no en la raiz junto a DismissTransients, porque decidirlo alli
        /// obligaria a la vista a preguntarle al modelo si la mano lleva algo para saber si
        /// cortar la propagacion. La vista informa; no consulta.</para>
        /// </summary>
        /// <returns>True si el clic ya se consumio cancelando.</returns>
        private bool RightClickCancelledGrab()
        {
            if (!_service.IsHandCarrying()) return false;

            CancelHand();
            return true;
        }

        private void OnCellRightPressed(GridPos pos, PanelType panel)
        {
            if (RightClickCancelledGrab()) return;

            IInventoryElement target = _panelPresenters[panel].GetNodeAt(pos);
            IEntity origin = _panelPresenters[panel].Entity;

            if (target == null || target.GetItemEntity() == null) 
                return;
                
            bool splittable = target.GetAmount() > 1
                           && !_service.FindSplitCell(origin.GetComponent<InventoryComponent>().Inventory,
                                                      target.GetItemEntity()).IsNone;

            bool hasSublots = target.HasVariants();
            
            List<ItemAction> actions = _service.GetAvailableActions(target.GetItemEntity(), _entity, origin,
                                                                    hasSublots, splittable);

            // El ancla se mide AQUI y no cuando se pulse la opcion: para entonces el evento de
            // puntero ya no existe y nadie sabe de que card salio el menu.
            RenderContextualMenu(actions, MenuContext.FromGrid(origin, target, panel, pos,
                                                               _panelPresenters[panel].ItemCornerAt(pos)));
        }

        private void OnEquipmentSlotRightClicked(int layer, bool fromLayersPopup = false)
        {
            if (RightClickCancelledGrab()) return;

            AC.CheckNotNegative(layer, nameof(layer));
 
            EquipmentSlotType type = CurrentLayerSlotType(fromLayersPopup);

            ItemEntity target = GarmentAt(type, layer);
            if (target == null)
                return;

            List<ItemAction> actions = _service.GetAvailableActions(target, _entity, _entity, false);

            RenderContextualMenu(actions, MenuContext.FromEquipment(_entity, target, type));
        }

        private EquipmentSlotType CurrentLayerSlotType(bool fromLayersPopup)
        {
            return fromLayersPopup
                ? _view.GetEquipmentLayerSlotType(_view.ActiveEquipmentSlot)
                : _view.GetEquipmentSlotType(_view.ActiveEquipmentSlot);
        }

        private int LayerToRealPos(int layer, int totalLayers) => totalLayers - 1 - layer; 
        /// <param name="context">De donde salio el menu. Ya no hace falta comprobar que trae
        /// algo sobre lo que actuar: sus fabricas lo garantizan al construirlo.</param>
        private void RenderContextualMenu(List<ItemAction> actions, MenuContext context)
        {
            _view.CloseContextualMenu();

            if (actions.Count == 0)
                return;

            List<MenuOption> options = new List<MenuOption>();
            foreach (ItemAction action in actions)
                options.AddRange(BuildOptions(action, context));

            _view.RenderContextualMenu(options);

            // Despues de renderizar, no antes: CloseContextualMenu avisa de su cierre y eso
            // despinta lo anterior. Fijar aqui deja el estado coherente con lo que se ve.
            SetPinned(context);
        }

        /// <summary>
        /// Traduce una accion posible a las entradas de menu que la representan.
        ///
        /// Devuelve una secuencia y no un MenuOption suelto porque una misma accion puede
        /// dar varias entradas: QuickTransfer se abre en una por cada inventario visible al
        /// que se pueda enviar. Las demas devuelven una sola.
        /// </summary>
        private IEnumerable<MenuOption> BuildOptions(ItemAction action, MenuContext context)
        {
            IInventoryElement target = context.Target;
            IEntity origin = context.Origin;

            switch (action)
            {
                case ItemAction.DropFromInventory:
                    { 
                        bool sublots = context.Item != null;
                        return new[] { new MenuOption("Tirar", inputs => OnDropItemRequested(target, origin, inputs.GetInt("amount"), context.Item), new List<MenuField> { MenuField.Int("amount", max: sublots ? target.GetAmount(context.Item) : target.GetAmount()) }) };
                    }
                    
                case ItemAction.Equip:
                    return BuildEquiOptions(target, origin, context.Item);

                case ItemAction.Unequip:
                    return new[] { new MenuOption("Desquipar", inputs => OnUnequipItemRequested(context.Item, origin, context.SlotType.Value), new List<MenuField>{})};

                case ItemAction.Consume:
                    return new[] { new MenuOption("Consumir", inputs => OnConsumeItemRequested(context.TargetStack, origin, context.Item), new List<MenuField>{}) };

                case ItemAction.QuickTransfer:
                    return BuildTransferOptions(target, origin, context.Item);

                case ItemAction.Inspect:
                    return new[] { new MenuOption("Inspeccionar", inputs => OnInspectItemRequested(context), new List<MenuField>{})};

                // max: Amount - 1 porque separar todo no separa nada.
                case ItemAction.Split:
                    return new[] { new MenuOption("Dividir",
                                                  inputs => OnSplitRequested(context, inputs.GetInt("amount")),
                                                  new List<MenuField> { MenuField.Int("amount", max: context.FocusedAmount - 1) })};

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), $"Sin MenuOption para {action}.");
            }
        }

        private IEnumerable<MenuOption> BuildInventoryTabsOptions(ItemEntity container)
        {
            List<MenuOption> subOptions = new List<MenuOption>()
            {
                new MenuOption("↑", inputs =>
                {

                    FocusInventory(container, PanelType.A);
                },
                new List<MenuField>()),
                new MenuOption("↓", inputs =>
                {
                    FocusInventory(container, PanelType.B);
                },
                new List<MenuField>())
            };

            List<MenuOption> options = new List<MenuOption>()
            {
                new MenuOption("Mostrar al lado", subOptions)
            };

            return options;
        }

        /// <summary>
        /// Lleva un contenedor al hueco pedido y lo quita de cualquier otro: un mismo inventario
        /// abierto dos veces serian dos vistas del mismo arbol, y mover algo en una dejaria la
        /// otra mintiendo hasta el siguiente repintado.
        ///
        /// <para>El barrido incluye el hueco de destino a proposito: asi pedir este contenedor
        /// SIEMPRE lo muestra aqui, en vez de alternar. La alternancia de ToggleExtraInventory
        /// se pierde por este camino, y es lo que se quiere — "mostrar al lado" es una orden,
        /// no un interruptor.</para>
        /// </summary>
        private void FocusInventory(ItemEntity container, PanelType targetPanel)
        {
            foreach (PanelType panelType in _panelPresenters.Keys)
            {
                // Identidad, no equivalencia: dos mochilas iguales y vacias son equivalentes y
                // cerrar la otra seria cerrar la que no es.
                if (ReferenceEquals(_panelPresenters[panelType].Entity, container))
                    CloseInventoryPanel(panelType);
            }

            ToggleExtraInventory(container, targetPanel);
        }

        /// <param name="variant">Sub-lote concreto a equipar, o null para el representante del
        /// nodo. Dos prendas del mismo tipo con desgaste distinto conviven en la misma pila, y
        /// equipar "una cualquiera" cuando el jugador señalo una seria elegir por el.</param>
        private IEnumerable<MenuOption> BuildEquiOptions(IInventoryElement target, IEntity origin, ItemEntity variant = null)
        {
            ItemEntity item = variant ?? target.GetItemEntity();

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            IReadOnlyList<EquipmentSlotType> dstSlotTypes = wearableComponent.TargetSlots;
            if (dstSlotTypes.Count == 0)
                throw new InvalidOperationException("Cannot try to equip an item with no posible slot targets");
            else if (dstSlotTypes.Count ==  1)
                return new[] { new MenuOption("Equipar (" + dstSlotTypes.First().GetDescription() + ")", inputs => OnEquipItemRequested(target, origin, dstSlotTypes.First(), variant), new List<MenuField>{})};
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

                return new[] { new MenuOption("Equipar (" + targetText + ")", inputs => OnEquipItemRequested(target, origin, dstSlotTypes.First(), variant), new List<MenuField>{})};
            }
            else
            {
                List<MenuOption> subOptions = new List<MenuOption>();
                foreach (EquipmentSlotType slotType in dstSlotTypes)
                {
                    subOptions.Add(new MenuOption(slotType.GetDescription(), inputs => OnEquipItemRequested(target, origin, slotType, variant), new List<MenuField>{}));
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
        private IEnumerable<MenuOption> BuildTransferOptions(IInventoryElement target, IEntity origin, ItemEntity variant)
        {
            List<MenuOption> destinies = new List<MenuOption>();

            foreach (PanelType panel in new[] { PanelType.Player, PanelType.A, PanelType.B })
            {
                IEntity destiny = _panelPresenters[panel].Entity;

                if (destiny == null || destiny == origin) continue;
                if (panel != PanelType.Player &&
                    !_view.IsSideContentVisible(panel, SidePanelContent.Inventory)) continue;

                destinies.Add(new MenuOption(DestinyName(destiny),
                                            inputs => OnQuickTransferRequested(target, origin, destiny, inputs.GetInt("amount"), variant),
                                            new List<MenuField>{ MenuField.Int("amount", variant != null ? target.GetAmount(variant) : target.GetAmount()) }));
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

        private void OnDropItemRequested(IInventoryElement target, IEntity origin, int amount, ItemEntity variant = null)
        {
            _service.DropItems(origin, target, amount, variant); 
        }

        /// <param name="variant">Sub-lote concreto, o null para el representante del nodo.</param>
        private void OnEquipItemRequested(IInventoryElement target, IEntity origin, EquipmentSlotType dstSlotType, ItemEntity variant = null)
        {
            ItemEntity item = variant ?? target.GetItemEntity();

            WearableComponent wearableComponent = item.GetComponent<WearableComponent>();
            IReadOnlyList<EquipmentSlotType> equipmentSlotTypes = wearableComponent.TargetSlots;
            if (!equipmentSlotTypes.Contains(dstSlotType))
                throw new InvalidOperationException("You cannot attempt to unequip an item from a slot where it could never be placed.");

            InventoryObject srcInventory = origin.GetComponent<InventoryComponent>().Inventory;

            _service.TryEquipItem(new InventoryNodeOrigin(origin, srcInventory, target),
                                  item,
                                  _entity,
                                  OccupiedSlots(item, dstSlotType));
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

        /// <summary>
        /// Separa unidades de la pila en una pila nueva del mismo inventario. El destino no se
        /// pregunta: la accion solo se ofrece cuando hay hueco, asi que aqui ya lo hay.
        /// </summary>
        private void OnSplitRequested(MenuContext context, int amount)
        {
            if (amount <= 0) return;

            _service.SplitNode(context.Origin, context.Target, context.Item, amount);
            _panelPresenters[context.Panel.Value].Refresh();
        }

        /// <param name="variant">Sub-lote concreto a consumir, o null para que lo elija el nodo.
        /// Importa: consumir una venda sucia no es lo mismo que consumir una limpia.</param>
        private void OnConsumeItemRequested(ItemObject target, IEntity origin, ItemEntity variant = null)
        {
            /*TODO fase2*/
        }

        /// <summary>
        /// El ancla sale del contexto, medida por quien abrio el menu: cada forma de abrirlo
        /// decide la suya, asi que aqui no se calcula.
        ///
        /// La lista de lotes se captura en el cierre y no se vuelve a leer: el indice que
        /// devuelve la vista indexa LA MISMA lista que se pinto. Si la variante ya no existe,
        /// GrabFrom la acota con Available y agarra cero — falla en vacio, no en otra variante.
        /// </summary>
        private void OnInspectItemRequested(MenuContext context)
        {
            ItemObject itemObject = context.TargetStack
                ?? throw new InvalidOperationException(
                    "Cannot inspect sub-lots of something that is not a stack (ItemObject - with multiple sublots): the action should not have been offered.");
            IReadOnlyList<SubLot> lots = itemObject.GetSubLots;

            List<ItemDisplayData> data = new List<ItemDisplayData>();
            foreach (SubLot sublot in lots)
                data.Add(DisplayDTOsBuilder.BuildDisplayData(sublot.Item, sublot.Amount));

            PanelType panel = context.Panel.Value;
            GridPos cell = context.Cell;

            _view.RenderSublotsPopup(data, context.Anchor,
                index => _panelPresenters[panel].GrabVariantAt(cell, lots[index].Item, lots[index].Amount),
                index => {
                    MenuContext sublotContext = MenuContext.FromSublot(context.Origin, itemObject, lots[index].Item, context.Panel.Value, context.Cell, context.Anchor);
                    RenderContextualMenu(_service.GetAvailableActions(lots[index].Item, _entity, _panelPresenters[panel].Entity, false), sublotContext);
                });
        }

        private void OnQuickTransferRequested(IInventoryElement target, IEntity origin, IEntity destiny, int amount, ItemEntity variant = null)
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
                    UpdateEquipmentRelated();
                    RefreshOpenLayers();
                    break;
                } 
            }
        }

        /// <summary>
        /// Repinta el popup de capas si esta abierto. Vive aqui y no en el boton porque el
        /// popup es una vista mas del equipo: lo que lo actualiza es que el equipo cambie,
        /// venga el cambio de donde venga.
        /// </summary>
        private void RefreshOpenLayers()
        {
            if (!_view.IsLayersPopupOpen) return;

            EquipmentSlotType type = _view.OpenLayersSlotType;
            EquipmentSlot slot = _entity.GetComponent<EquipmentComponent>().GetEquipmentSlot(type);

            // Sin capas por debajo de la superior no hay nada que enseñar, y el boton que lo
            // abre tampoco estaria visible: cerrarlo es lo unico coherente.
            if (slot.GetEquippedItemCount() <= 1) { _view.CloseLayersPopup(); return; }

            OnSlotLayersRequested(type);
        }

        

        private void UpdateInventoryTabs(EquipmentComponent equipmentComponent)
        { 
            _view.ClearInveotryTabs();

            _view.AddTabToInventoryTabs(_entity.GetName(), () => _panelPresenters[PanelType.Player].Bind(_entity), new List<MenuOption>());
            
            // EquippedItems y no un recorrido por slots: una prenda de ocupacion completa esta
            // en varios a la vez, y saldria con una pestaña por slot.
            foreach (ItemEntity item in equipmentComponent.EquippedItems())
            {
                if (item.GetComponent<InventoryComponent>() == null) continue;

                _view.AddTabToInventoryTabs(item.GetDisplayName(),
                                            () => _panelPresenters[PanelType.Player].Bind(item),
                                            BuildInventoryTabsOptions(item).ToList());
            }

            _view.RenderInventoryTabs(() => _panelPresenters[PanelType.Player].Bind(_entity)); 
        } 

        private void UpdateEquipmentRelated()
        {
            EquipmentComponent equipmentComponent = _entity.GetComponent<EquipmentComponent>();
            _view.UpdateEquipmentSlots(equipmentComponent);
            UpdateInventoryTabs(equipmentComponent);
        }
    }
}
