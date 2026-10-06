using UnityEngine;
using Strategy;
using MVC.Controller;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.MVC.Presenter;
using Core.MVC.Presenter.Inventory;
using Core.MVC.View;
using MVC.View;
using Core;
using Core.Contexts;
using Core.Item;
using Core.Factories;
using System.Collections.Generic;
using Core.Services;
using MVC.View.Inventory;
using MVC.View.World;
using Core.MVC.Presenter.World;
using Core.MVC.Presenter.HUD;
using MVC.View.HUD;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.Inventory;


/// <summary>
/// Unity entry point. Builds the contexts, links entities and starts up.
/// </summary>
public class GameMain : MonoBehaviour
{
    [SerializeField] private UIRegistry _uiRegistry;

    private GameController _gameController;
    private GameContext _gameContext;
    private int _lastRebuildFrame = -1;

    // If the number of services grows, make a service locator ( with dictionaries )
    private InventoryService _inventoryService;
    private WorldInteractionService _worldInteractionService;

    // La misma instancia que usa InputManager, no una copia: la vista de interaccion la
    // necesita para proyectar a pantalla con la camara activa. No va al contexto por la
    // misma razon que no iba antes.
    private CameraRegister _cameraRegister;


    void Awake()
    {
        _gameContext = new GameContext();

        BootstrapCore();

        GameDataContext dataCtx = BuildDataContext();

        GameSessionContext sessionCtx = BuildSessionContext(dataCtx);
        if (sessionCtx == null) return;

        GameSystemContext systemCtx = BuildSystemContext(dataCtx._entityManager);

        // Link contexts to super context (game context). The interaction context is built
        // here and never again: BuildViewsAndPresenters runs on every UI live reload, and
        // rebuilding it there would drop whatever the hand is holding.
        _gameContext.SetData(dataCtx)
                    .SetSession(sessionCtx)
                    .SetSystem(systemCtx)
                    .SetInteraction(new GameInteractionContext());

        BuildUnityPieces(sessionCtx._player, systemCtx.PresenterManager);

        // GameController receives only what it needs
        _gameController = new GameController(systemCtx, _gameContext.InputManager);

        UIReloadNotifier.OnUIRecreated += BuildViewsAndPresenters;
        BuildServices();
        BuildViewsAndPresenters();
    }

    void Update()
    {
        _gameController.Update(Time.deltaTime);
    }

    /// <summary>
    /// Static wiring Core classes rely on. Runs first: the catalog loader resolves its
    /// paths through CoreConfig, so it cannot be built before this.
    /// </summary>
    private void BootstrapCore()
    {
        CoreLogger.Instance = new Unity.UnityLogger();
        CoreConfig.BasePath = Application.streamingAssetsPath;
    }

    /// <summary>
    /// World data: item catalog loaded from JSON and the entity manager built on it.
    /// </summary>
    private GameDataContext BuildDataContext()
    {
        ItemCatalogue itemCatalogue = new ItemCatalogue();
        JsonItemCatalogLoader jsonItemCatalogLoader = new JsonItemCatalogLoader();
        jsonItemCatalogLoader.LoadInto(itemCatalogue);
        itemCatalogue.LogCatalogContents();

        EntityManager entityManager = new EntityManager(new PrototypeFactory(itemCatalogue));

        return new GameDataContext(entityManager, itemCatalogue);
    }

    /// <summary>
    /// Current session state: creates the player entity and links it to its GameObject.
    /// Returns null if the player could not be created — startup cannot continue.
    /// </summary>
    private GameSessionContext BuildSessionContext(GameDataContext dataContext)
    {
        IEntity player = dataContext._entityManager.CreateEntity(EntityType.Player);
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return null;
        }

        // Link Core entity with Unity GameObject via specialized linker
        IEntityLinker linker = new Unity.UnityEntityLinker();
        linker.Link(player, EntityType.Player);

        GameSessionContext sessionCtx = new GameSessionContext();
        sessionCtx.SetPlayer(player); 
        AddDevTestingPlayerItems(player, dataContext._itemCatalogue);
        AddDevTestingExtraInventories(sessionCtx, dataContext);
        return sessionCtx;
    }

    /// <summary>
    /// Dev: objetos y ropa iniciales del jugador para probar rejilla y equipo sin recoger
    /// nada. Vivian en PrototypeFactory, y eso hacia que EntityManager no pudiera construirse
    /// sin el catalogo real (los pide por nombre), lo que bloqueaba los tests del mundo.
    /// Aqui son una llamada que se quita sola el dia que el jugador empiece sin nada.
    /// </summary>
    private void AddDevTestingPlayerItems(IEntity player, ItemCatalogue catalogue)
    {
        EquipmentComponent equipment = player.GetComponent<EquipmentComponent>();
        equipment.EquipItem(EquipmentSlotType.Chest, catalogue.CreateItem("Camisa"));
        equipment.EquipItem(EquipmentSlotType.Chest, catalogue.CreateItem("Pechera"));

        InventoryObject inv = player.GetComponent<InventoryComponent>().Inventory;
        AddDevTestingItem(inv, catalogue, "Espada de hierro", 1);
        AddDevTestingItem(inv, catalogue, "Arco corto", 1);
        AddDevTestingItem(inv, catalogue, "Manzana", 5);
        AddDevTestingItem(inv, catalogue, "Manzana", 5, 87);
        AddDevTestingItem(inv, catalogue, "Manzana", 5, 31);
        AddDevTestingItem(inv, catalogue, "Venda", 3);
        AddDevTestingItem(inv, catalogue, "Odre", 1);
    }

    private void AddDevTestingItem(InventoryObject inv, ItemCatalogue catalogue, string itemName,
                                   int amount, int durability = 100)
    {
        ItemEntity item = catalogue.CreateItem(itemName);
        item.GetComponent<BaseItemComponent>().SetDurability(durability);
        int remaining = inv.AddItem(item, amount);
        if (remaining > 0)
            Debug.LogWarning($"GameMain: no room for {remaining}x '{itemName}' in the dev test inventory.");
    }

    private void AddDevTestingExtraInventories(GameSessionContext sessionContext, GameDataContext dataContext)
    { 
        ItemEntity itemA = dataContext._itemCatalogue.CreateItem("Arcón pequeño");
        ItemEntity itemB = dataContext._itemCatalogue.CreateItem("Arcón pequeño");
        NameComponent nameComponentA = new NameComponent();
        NameComponent nameComponentB = new NameComponent();
        nameComponentA.SetDisplayName("Cofre A - inventario de prueba");
        nameComponentB.SetDisplayName("Cofre B - inventario de prueba");
        itemA.AddComponent(nameComponentA);
        itemB.AddComponent(nameComponentB);

        sessionContext.SetFirstInventorySrc(itemA);
        sessionContext.SetSecondInventorySrc(itemB);

    }


    /// <summary>
    /// Infrastructure: the system manager with every system registered, and the presenter
    /// registry. Registering a reactive system also subscribes it to the event bus.
    /// </summary>
    private GameSystemContext BuildSystemContext(EntityManager entityManager)
    {
        SystemManager systemManager = new SystemManager(entityManager);

        systemManager.RegisterPeriodicGameSystem(new FatigueStaminaSystem());
        systemManager.RegisterEngineSystem(new Unity.TransformSyncSystem());
        systemManager.RegisterReactiveGameSystem(new MovementSystem())
                     .RegisterReactiveGameSystem(new InventorySystem())
                     .RegisterReactiveGameSystem(new EquipmentSystem())
                     .RegisterReactiveGameSystem(new WorldInteractionSystem(entityManager, new Unity.UnityEntityLinker(),
                                                                            new Unity.LineOfSightFilter()));

        PresenterManager presenterManager = new PresenterManager();

        return new GameSystemContext(systemManager, presenterManager);
    }

    /// <summary>
    /// Unity-only pieces that do not belong in Core: cameras and input (the HUD is a
    /// presenter now, built with the other views). Input is handed to the game context;
    /// CameraRegister deliberately is not
    /// (it self-instantiates its cameras and a stored copy would drift from this one), so
    /// it stays local — only InputManager and the startup activation need it.
    /// </summary>
    private void BuildUnityPieces(IEntity player, PresenterManager presenterManager)
    {
        CameraRegister cameraRegister = new CameraRegister();
        _cameraRegister = cameraRegister;
        InputManager inputManager = new InputManager(cameraRegister, presenterManager, _gameContext.Session);

        cameraRegister.InitizalizeCameras(player);
        cameraRegister.ActivateCamera(CameraRegister.CameraType.RTS);

        _gameContext.SetInputManager(inputManager);
    }

    private void BuildServices()
    {
        _inventoryService = new InventoryService(_gameContext.Interaction, _gameContext.System);
        _worldInteractionService = new WorldInteractionService(_gameContext.System);
    }

   private void BuildViewsAndPresenters()
    {
        if (_lastRebuildFrame == Time.frameCount) return;
        _lastRebuildFrame = Time.frameCount;

        PresenterManager presenters = _gameContext.System.PresenterManager;

        IPresenter old = presenters.GetPresenter<IPresenter>(PresenterType.INV);
        bool wasOpen = old != null && old.IsOpen();

        ViewManager viewManager = new ViewManager();
        viewManager.InitializeViews(_uiRegistry, _cameraRegister);

        InventoryPresenter presenter = new InventoryPresenter(viewManager.GetView<InventoryView>(PresenterType.INV),
                                                              _gameContext.Data._itemCatalogue,
                                                              _inventoryService);
        presenters.ReplacePresenter(PresenterType.INV, presenter);

        if (wasOpen) presenter.Open(_gameContext.Session._player);

        // No se reabre aqui: InputManager lo abre o cierra cada fotograma segun la camara y
        // el inventario, asi que tras una recarga se recupera solo en el siguiente.
        // Recibe el inventario recien creado como IContainerPanels: se crean juntos aqui, asi
        // que tras una recarga el mundo nuevo apunta al inventario nuevo y no hay orden que
        // respetar ni suscripcion que olvidar.
        WorldInteractionPresenter worldPresenter = new WorldInteractionPresenter(
            viewManager.GetView<WorldInteractionView>(PresenterType.WORLD),
            _worldInteractionService,
            presenter);
        presenters.ReplacePresenter(PresenterType.WORLD, worldPresenter);

        // El viejo se cierra antes de sustituirlo: asi se da de baja del EventBus y no sigue
        // pintando sobre elementos huerfanos. No se reabre aqui: lo abre InputManager segun
        // la camara, igual que la interaccion con el mundo.
        presenters.GetPresenter<IPresenter>(PresenterType.HUD)?.Close(true);
        HUDPresenter hudPresenter = new HUDPresenter(viewManager.GetView<HUDView>(PresenterType.HUD));
        presenters.ReplacePresenter(PresenterType.HUD, hudPresenter);
    }

    private void OnDestroy() => UIReloadNotifier.OnUIRecreated -= BuildViewsAndPresenters;
}
