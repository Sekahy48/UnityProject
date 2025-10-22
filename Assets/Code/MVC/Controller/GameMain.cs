using UnityEngine;
using Strategy;
using MVC.Controller;
using ECS.Entity;
using Factories;

/// <summary>
/// Main entry point for the game. Initializes the game context and input manager.
/// </summary>
/// 
public class GameMain : MonoBehaviour
{
    private GameContext gameContext;
    private InputManager inputManager;

    // TEMPORAL
    private IEntity playerEntity;
    // TEMPORAL
    void Awake()
    {
        
        // Establecemos el GameContext y sus componentes
        gameContext = new GameContext();
        gameContext.SetGameController(new GameController(gameContext, this)).SetLogic(new MVC.Model.Logic()).SetInputManager(inputManager = new InputManager(gameContext));
        //GameObject.FindWithTag("MainCamera").GetComponent<Camera>().enabled = false; // Desactivamos la cámara por defecto de Unity
        // Gestionamos el MainPlayer TODO montar un sistema de login y de gestion modular respecto al player
        IEntity player = gameContext.GetLogic().GetEntityManager().CreateEntity("playerEntity");
        if (player == null)
        {
            Debug.LogError("Player entity could not be created.");
            return;
        }

        gameContext.SetHUDManager(new MVC.View.HUDManager(player));
        
        gameContext.GetCameraRegister().AddCamera(CameraRegister.CameraType.RTS, new RTSCameraStrategy());
        gameContext.GetCameraRegister().AddCamera(CameraRegister.CameraType.FPS, new FirstPersonCamera(player));
        gameContext.GetCameraRegister().AddCamera(CameraRegister.CameraType.TPS, new ThirdPersonCamera(player)); 
        gameContext.GetCameraRegister().ActivateCamera(CameraRegister.CameraType.RTS);

        // Inicializamos el InputManager
        inputManager = new InputManager(gameContext);
        gameContext.GetGameController().SetUpOnStart();
    }

    void Update()
    {
        //Debug.Log("GameMain Update called.");
        // Lógica del juego
        //Debug.Log(gameContext);
        gameContext.GetGameController().Update(Time.deltaTime);
        gameContext.GetLogic().UpdateThis();
    }
}
