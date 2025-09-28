using System.Runtime.InteropServices;
using Strategy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MVC.Controller
{
    public class InputManager
    {
        private GameContext GameContext;
        private ICameraStrategy activeCam;

        public InputManager(GameContext gameContext)
        {
            this.GameContext = gameContext;
        }

        public void Update(float deltaTime)
        {
            if (activeCam == null)
            {
                activeCam = GameContext.GetCameraRegister().GetActiveCamera();
                if (activeCam == null)
                {
                    Debug.LogError("No active camera strategy found in InputManager.");
                    return;
                }
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                activeCam = GameContext.GetCameraRegister().NextCamera();
            }

            
            activeCam.Execute(deltaTime);
        }
        /*
        public ICameraStrategy GetCameraStrategy(CameraRegister.CameraType camera)
        {
            ICameraStrategy outCam = GameContext.GetCameraRegister().GetCamera(camera);
            outCam.activate();
            
            return outCam;
        }
        */
    }
}       