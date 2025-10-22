using System.Runtime.InteropServices;
using ECS.Entity;
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
                Debug.Log("Switching to next camera.");
                activeCam = GameContext.GetCameraRegister().NextCamera();
            }

            activeCam.Execute(deltaTime);
            IEntity player = activeCam.GetPlayer();
            if (player != null)
            {
                bool isRunning = ((Strategy.BaseCameraStrategy)this.activeCam).GetMov().IsRunning();
                this.GameContext.GetLogic().GetFatigueStaminaSystem().ProcessEntity(deltaTime, player, isRunning);
            }
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