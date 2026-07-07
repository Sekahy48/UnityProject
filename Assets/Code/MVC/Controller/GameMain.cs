using UnityEngine;
using Strategy;
using MVC.Controller;
using ECS.Entity;
using ECS.Systems;
using Factories;
using MVC.Presenter;
using MVC.Presenter.Inventory;
using MVC.View;

/// <summary>
/// Main entry point for the game. Initializes the game context and input manager.
/// </summary>
public class GameMain : MonoBehaviour
{
    private GameContext gameContext;
    private InputManager inputManager;
    [SerializeField] private UIRegistry _uiRegistry;

    void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("MainPlayer");

        gameContext = new GameContext();
        gameContext.SetGameController(new GameController(gameContext, this))
                   .SetLogic(new MVC.Model.Logic(playerObj))
                   .SetInputManager(inputManager = new InputManager(gameContext))
                   .SetUIRegistry(_uiRegistry);

        // Crear entidad del jugador
        IEntity player = gameContext.GetLogic().GetEntityManager().CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        gameContext.SetHUDManager(new MVC.View.HUDManager(player));

        // Cámaras
        gameContext.GetCameraRegister().InitizalizeCameras(player);
        gameContext.GetCameraRegister().ActivateCamera(CameraRegister.CameraType.RTS);

        // SystemManager: registrar game systems
        var systemManager = new SystemManager(gameContext.GetLogic().GetEntityManager());
        systemManager.RegisterGameSystem(new FatigueStaminaSystem());
        gameContext.SetSystemManager(systemManager);

        // Configurar observers y demás
        gameContext.GetGameController().SetUpOnStart();

        // Vistas y presenters
        gameContext.GetViewManager().InitializeViews(_uiRegistry);
        this.InitializePresenters();
    }

    void Update()
    {
        gameContext.GetGameController().Update(Time.deltaTime);
    }

    private void InitializePresenters()
    {
        PresenterManager presenterManager = gameContext.GetPresenterManager();
        presenterManager.RegisterPresenter(PresenterType.INV,
            new InventoryPresenter(gameContext.GetViewManager().GetView<InventoryView>(PresenterType.INV)));
    }
}
