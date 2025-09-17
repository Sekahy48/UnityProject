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
        gameContext.SetGameController(new GameController(gameContext, this))/*TODO*/.SetLogic(new MVC.Model.Logic())/*TODO*/;
        gameContext.GetCameraRegister().AddCamera(CameraRegister.CameraType.RTS, new RTSCameraStrategy());
        gameContext.GetCameraRegister().AddCamera(CameraRegister.CameraType.FPS, new FPCameraStrategy(null));
        gameContext.GetCameraRegister().ActivateCamera(CameraRegister.CameraType.RTS);

        // Gestionamos el MainPlayer TODO montar un sistema de login y de gestion modular respecto al player
        gameContext.GetLogic().GetEntityManager().CreateEntity("playerEntity");
        
        // Inicializamos el InputManager
        inputManager = new InputManager(gameContext);
    }

    void Update()
    {
        // Lógica del juego
        gameContext.GetGameController().Update(Time.deltaTime);
    }
}
