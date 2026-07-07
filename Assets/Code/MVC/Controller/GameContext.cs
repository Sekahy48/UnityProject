using ECS.Systems;
using MVC.Model;
using MVC.Presenter;
using MVC.View;
using Strategy;
using UnityEngine;

namespace MVC.Controller
{
    public class GameContext
    {
        private GameController GameController;
        private CameraRegister CameraRegister = new();
        private InputManager InputManager;
        private HUDManager HUDManager;
        private SystemManager SystemManager;

        private PresenterManager PresenterManager = new();
        private ViewManager ViewManager = new();

        private UIRegistry UIRegistry;

        private Logic Model;
        public GameContext(GameController gameController, Logic model)
        {
            GameController = gameController;
            Model = model;
        }

        public GameContext()
        {
        }

        public GameContext SetLogic(Logic logic)
        {
            Model = logic;
            return this;
        }

        public Logic GetLogic()
        {
            return Model;
        }

        public GameContext SetGameController(GameController gameController)
        {
            GameController = gameController;
            return this;
        }

        public GameContext SetInputManager(InputManager inputManager)
        {
            InputManager = inputManager;
            return this;
        }

        public GameContext SetHUDManager(HUDManager hudManager)
        {
            HUDManager = hudManager;
            return this;
        }

        public GameContext SetUIRegistry(UIRegistry uiRegistry)
        {
            UIRegistry = uiRegistry;
            return this;
        }

        public GameController GetGameController()
        {
            return GameController;
        }

        public CameraRegister GetCameraRegister()
        {
            return CameraRegister;
        }

        public ICameraStrategy GetCamera(CameraRegister.CameraType name)
        {
            return CameraRegister.GetCamera(name);
        }
        public InputManager GetInputManager()
        {
            return InputManager;
        }
        
        public HUDManager GetHUDManager()
        {
            return HUDManager;
        }

        public PresenterManager GetPresenterManager()
        {
            return PresenterManager;
        }

        public ViewManager GetViewManager()
        {
            return ViewManager;
        }

        public GameContext SetSystemManager(SystemManager systemManager)
        {
            SystemManager = systemManager;
            return this;
        }

        public SystemManager GetSystemManager()
        {
            return SystemManager;
        }
    }
}