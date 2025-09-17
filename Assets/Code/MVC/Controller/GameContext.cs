using MVC.Model;
using Strategy;
using UnityEngine;

namespace MVC.Controller
{
    public class GameContext
    {
        private GameController GameController;
        private Logic Model;
        private CameraRegister CameraRegister = new();
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
         
    }
}