using System.Runtime.InteropServices;
using Strategy;
using Unity.VisualScripting;

namespace MVC.Controller
{
    public class InputManager
    {
        private GameContext GameContext; 

        public InputManager(GameContext gameContext)
        {
            this.GameContext = gameContext;

        }

        public ICameraStrategy GetCameraStrategy(CameraRegister.CameraType camera)
        {
            ICameraStrategy outCam = GameContext.GetCameraRegister().GetCamera(camera);
            outCam.activate();
            
            return outCam;
        }
    }
}       