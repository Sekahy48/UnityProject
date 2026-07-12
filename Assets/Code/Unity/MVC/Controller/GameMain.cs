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

/// <summary>
/// Unity entry point. Builds the contexts, links entities and starts up.
/// </summary>
public class GameMain : MonoBehaviour
{
    [SerializeField] private UIRegistry _uiRegistry;

    private GameController gameController;

    void Awake()
    {
        // ---- Create Core contexts ----
        var logic = new MVC.Model.Logic();
        var entityManager = logic.GetEntityManager();

        IEntity player = entityManager.CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        // Link Core entity with Unity GameObject via specialized linker
        IEntityLinker linker = new Unity.UnityEntityLinker();
        linker.Link(player, "playerEntity");

        // Configure static logger for Core classes
        CoreLogger.Instance = new Unity.UnityLogger();

        var dataCtx = new GameDataContext(entityManager);

        var sessionCtx = new GameSessionContext();
        sessionCtx.SetPlayer(player);

        // SystemManager: register systems
        var systemManager = new SystemManager(entityManager);
        systemManager.RegisterGameSystem(new FatigueStaminaSystem());
        systemManager.RegisterEngineSystem(new Unity.TransformSyncSystem());

        var presenterManager = new PresenterManager();
        var systemCtx = new GameSystemContext(systemManager, presenterManager);

        // ---- Unity pieces ----
        var hudManager = new HUDManager(player);
        var cameraRegister = new CameraRegister();
        var inputManager = new InputManager(cameraRegister, presenterManager);

        // Cameras
        cameraRegister.InitizalizeCameras(player);
        cameraRegister.ActivateCamera(CameraRegister.CameraType.RTS);

        // GameController receives only what it needs
        gameController = new GameController(systemCtx, inputManager, hudManager);
        gameController.SetUpOnStart();

        // Views and presenters
        var viewManager = new ViewManager();
        viewManager.InitializeViews(_uiRegistry);
        presenterManager.RegisterPresenter(PresenterType.INV,
            new InventoryPresenter(viewManager.GetView<InventoryView>(PresenterType.INV)));
    }

    void Update()
    {
        gameController.Update(Time.deltaTime);
    }
}
