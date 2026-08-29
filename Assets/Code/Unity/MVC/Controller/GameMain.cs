using UnityEngine;
using Strategy;
using MVC.Controller;
using ECS.Entity;
using ECS.Systems;
using MVC.Presenter;
using MVC.Presenter.Inventory;
using MVC.View;
using Core;
using Core.Contexts;
using Item;
using Factories;
using System.Collections.Generic;
using Services;
using MVC.View.Inventory;
using ECS.Component;


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
        _gameController = new GameController(systemCtx,
                                             _gameContext.InputManager,
                                             _gameContext.HUDManager);
        _gameController.SetUpOnStart();

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
        IEntity player = dataContext._entityManager.CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return null;
        }

        // Link Core entity with Unity GameObject via specialized linker
        IEntityLinker linker = new Unity.UnityEntityLinker();
        linker.Link(player, "playerEntity");

        GameSessionContext sessionCtx = new GameSessionContext();
        sessionCtx.SetPlayer(player); 
        AddDevTestingExtraInventories(sessionCtx, dataContext);
        return sessionCtx;
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
                     .RegisterReactiveGameSystem(new InventorySystem());

        PresenterManager presenterManager = new PresenterManager();

        return new GameSystemContext(systemManager, presenterManager);
    }

    /// <summary>
    /// Unity-only pieces that do not belong in Core: HUD, cameras and input.
    /// HUD and input are handed to the game context; CameraRegister deliberately is not
    /// (it self-instantiates its cameras and a stored copy would drift from this one), so
    /// it stays local — only InputManager and the startup activation need it.
    /// </summary>
    private void BuildUnityPieces(IEntity player, PresenterManager presenterManager)
    {
        HUDManager hudManager = new HUDManager(player);
        CameraRegister cameraRegister = new CameraRegister();
        InputManager inputManager = new InputManager(cameraRegister, presenterManager, _gameContext.Session);

        cameraRegister.InitizalizeCameras(player);
        cameraRegister.ActivateCamera(CameraRegister.CameraType.RTS);

        _gameContext.SetHUDManager(hudManager)
                    .SetInputManager(inputManager);
    }

    private void BuildServices()
    {
        _inventoryService = new InventoryService(_gameContext.Interaction, _gameContext.System);
    }

   private void BuildViewsAndPresenters()
    {
        if (_lastRebuildFrame == Time.frameCount) return;
        _lastRebuildFrame = Time.frameCount;

        PresenterManager presenters = _gameContext.System.PresenterManager;

        IPresenter old = presenters.GetPresenter<IPresenter>(PresenterType.INV);
        bool wasOpen = old != null && old.IsOpen();

        ViewManager viewManager = new ViewManager();
        viewManager.InitializeViews(_uiRegistry);

        InventoryPresenter presenter = new InventoryPresenter(viewManager.GetView<InventoryView>(PresenterType.INV),
                                                              _gameContext.Data._itemCatalogue,
                                                              _inventoryService);
        presenters.ReplacePresenter(PresenterType.INV, presenter);

        if (wasOpen) presenter.Open(_gameContext.Session._player);
    }

    private void OnDestroy() => UIReloadNotifier.OnUIRecreated -= BuildViewsAndPresenters;
}
