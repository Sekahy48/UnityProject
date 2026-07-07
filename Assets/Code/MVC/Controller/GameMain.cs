using UnityEngine;
using Strategy;
using MVC.Controller;
using ECS.Entity;
using Factories;
using MVC.Presenter;
using MVC.Presenter.Inventory;
using MVC.View;

/// <summary>
/// Main entry point for the game. Initializes the game context and input manager.
/// </summary>
/// 
public class GameMain : MonoBehaviour
{
    private GameContext gameContext;
    private InputManager inputManager;
    [SerializeField] private UIRegistry _uiRegistry;
    // TEMPORAL
    private IEntity playerEntity;
    // TEMPORAL
    void Awake()
    {
        
        // Establecemos el GameContext y sus componentes
        GameObject playerObj = GameObject.FindWithTag("MainPlayer");

        gameContext = new GameContext();
        gameContext.SetGameController(new GameController(gameContext, this)).SetLogic(new MVC.Model.Logic(playerObj))
                                                                            .SetInputManager(inputManager = new InputManager(gameContext))
                                                                            .SetUIRegistry(_uiRegistry);
        //GameObject.FindWithTag("MainCamera").GetComponent<Camera>().enabled = false; // Desactivamos la cámara por defecto de Unity
        // Gestionamos el MainPlayer TODO montar un sistema de login y de gestion modular respecto al player
        IEntity player = gameContext.GetLogic().GetEntityManager().CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        gameContext.SetHUDManager(new MVC.View.HUDManager(player));
        
        gameContext.GetCameraRegister().InitizalizeCameras(player);
        gameContext.GetCameraRegister().ActivateCamera(CameraRegister.CameraType.RTS);
         
        gameContext.GetGameController().SetUpOnStart();


        // Inicializar vistas y presenters
        gameContext.GetViewManager().InitializeViews(_uiRegistry);
        this.InitializePresenters();
    }

    void Update()
    {
        //Debug.Log("GameMain Update called.");
        // Lógica del juego
        //Debug.Log(gameContext);
        gameContext.GetGameController().Update(Time.deltaTime);
        gameContext.GetLogic().UpdateThis();
    }

    private void InitializePresenters()
    {
        PresenterManager presenterManager = gameContext.GetPresenterManager();
        presenterManager.RegisterPresenter(PresenterType.INV, new InventoryPresenter(gameContext.GetViewManager().GetView<InventoryView>(PresenterType.INV)));
        
    }


}
