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


/// <summary>
/// Unity entry point. Builds the contexts, links entities and starts up.
/// </summary>
public class GameMain : MonoBehaviour
{
    [SerializeField] private UIRegistry _uiRegistry;

    private GameController _gameController;
    private GameContext _gameContext;
    private int _lastRebuildFrame = -1;


    void Awake()
    {
        _gameContext = new GameContext();

        // Configure static logger and paths for Core classes
        CoreLogger.Instance = new Unity.UnityLogger();
        CoreConfig.BasePath = Application.streamingAssetsPath;
        
        // ---- Create Core contexts ----
        ItemCatalogue itemCatalogue = new ItemCatalogue();
        JsonItemCatalogLoader jsonItemCatalogLoader = new JsonItemCatalogLoader();
        jsonItemCatalogLoader.LoadInto(itemCatalogue);
        itemCatalogue.LogCatalogContents();


        EntityManager entityManager = new EntityManager(new PrototypeFactory(itemCatalogue)); 

        IEntity player = entityManager.CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        // Link Core entity with Unity GameObject via specialized linker
        IEntityLinker linker = new Unity.UnityEntityLinker();
        linker.Link(player, "playerEntity");

        var dataCtx = new GameDataContext(entityManager, itemCatalogue);

        var sessionCtx = new GameSessionContext();
        sessionCtx.SetPlayer(player);

        // SystemManager: register systems
        var systemManager = new SystemManager(entityManager); 
        systemManager.RegisterPeriodicGameSystem(new FatigueStaminaSystem()); 
        systemManager.RegisterEngineSystem(new Unity.TransformSyncSystem());
        systemManager.RegisterReactiveGameSystem(new MovementSystem())
                     .RegisterReactiveGameSystem(new InventorySystem());

        var presenterManager = new PresenterManager();
        var systemCtx = new GameSystemContext(systemManager, presenterManager); 

        // ---- Unity pieces ----
        var hudManager = new HUDManager(player);
        var cameraRegister = new CameraRegister();
        var inputManager = new InputManager(cameraRegister, presenterManager);

        // Link contexts to super context (game context)
        _gameContext.SetData(dataCtx)
                    .SetSession(sessionCtx)
                    .SetSystem(systemCtx)
                    .SetHUDManager(hudManager)
                    .SetInputManager(inputManager);

        // Cameras
        cameraRegister.InitizalizeCameras(player);
        cameraRegister.ActivateCamera(CameraRegister.CameraType.RTS);

        // GameController receives only what it needs
        _gameController = new GameController(systemCtx, inputManager, hudManager);
        _gameController.SetUpOnStart(); 

        UIReloadNotifier.OnUIRecreated += BuildViewsAndPresenters;
        BuildViewsAndPresenters();
        
    }

    void Update()
    {
        _gameController.Update(Time.deltaTime);
    }

   private void BuildViewsAndPresenters()
    {
        if (_lastRebuildFrame == Time.frameCount) return;
        _lastRebuildFrame = Time.frameCount;

        PresenterManager presenters = _gameContext.System.PresenterManager;

        IPresenter old = presenters.GetPresenter<IPresenter>(PresenterType.INV);
        bool wasOpen = old != null && old.IsOpen();

        var viewManager = new ViewManager();
        viewManager.InitializeViews(_uiRegistry);

        var presenter = new InventoryPresenter(viewManager.GetView<InventoryView>(PresenterType.INV),
                                               _gameContext.Data._itemCatalogue);
        presenters.ReplacePresenter(PresenterType.INV, presenter);

        if (wasOpen) presenter.Open(_gameContext.Session.Player);
    }

    private void OnDestroy() => UIReloadNotifier.OnUIRecreated -= BuildViewsAndPresenters;
}
