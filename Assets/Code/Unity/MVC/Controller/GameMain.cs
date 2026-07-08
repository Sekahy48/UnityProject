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
/// Punto de entrada Unity. Construye los contextos, vincula entidades y arranca.
/// </summary>
public class GameMain : MonoBehaviour
{
    [SerializeField] private UIRegistry _uiRegistry;

    private GameController gameController;

    void Awake()
    {
        // ---- Crear contextos Core ----
        var logic = new MVC.Model.Logic();
        var entityManager = logic.GetEntityManager();

        IEntity player = entityManager.CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        // Vincular entidad Core con GameObject Unity via linker especializado
        IEntityLinker linker = new Unity.UnityEntityLinker();
        linker.Link(player, "playerEntity");

        var dataCtx = new GameDataContext(entityManager);

        var sessionCtx = new GameSessionContext();
        sessionCtx.SetPlayer(player);

        // SystemManager: registrar sistemas
        var systemManager = new SystemManager(entityManager);
        systemManager.RegisterGameSystem(new FatigueStaminaSystem());
        systemManager.RegisterEngineSystem(new Unity.TransformSyncSystem());

        var presenterManager = new PresenterManager();
        var systemCtx = new GameSystemContext(systemManager, presenterManager);

        // ---- Piezas Unity ----
        var hudManager = new HUDManager(player);
        var cameraRegister = new CameraRegister();
        var inputManager = new InputManager(cameraRegister, presenterManager);

        // Cámaras
        cameraRegister.InitizalizeCameras(player);
        cameraRegister.ActivateCamera(CameraRegister.CameraType.RTS);

        // GameController recibe solo lo que necesita
        gameController = new GameController(systemCtx, inputManager, hudManager);
        gameController.SetUpOnStart();

        // Vistas y presenters
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
