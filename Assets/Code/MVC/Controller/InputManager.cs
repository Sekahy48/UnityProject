using MVC.Presenter;
using MVC.Presenter.Inventory;
using Strategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MVC.Controller
{
    public class InputManager
    {
        private GameContext GameContext;
        private ICameraStrategy _activeStrategy;  
        public InputManager(GameContext gameContext)
        {
            this.GameContext = gameContext;  
        }

        public void Update(float deltaTime)
        {
            if (_activeStrategy == null)
            {
                _activeStrategy = GameContext.GetCameraRegister().GetActiveCamera();
                if (_activeStrategy == null)
                {
                    Debug.LogError("No active camera strategy found in InputManager.");
                    return;
                }
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            { 

                Debug.Log("Switching to next camera.");
                ICameraStrategy nextStrategy = GameContext.GetCameraRegister().NextCamera();
                this.SetActiveStrategy(nextStrategy);
            }

            _activeStrategy.Execute(deltaTime);
        }
 
        public void SetActiveStrategy(ICameraStrategy strategy)
        {
            // Desuscribir la anterior si existe
            if (_activeStrategy != null && _activeStrategy is IInventoryInputSource invStrategy)
                invStrategy.OnInventoryRequested -= OnInventoryRequested;

            _activeStrategy = strategy;

            // Suscribir la nueva
            if (_activeStrategy is IInventoryInputSource invStrategy2)
                invStrategy2.OnInventoryRequested += OnInventoryRequested;

            else
            {
                InventoryPresenter presenter = this.GameContext.GetPresenterManager()
                .GetPresenter<InventoryPresenter>(PresenterType.INV);
                presenter.Close();
            }
            _activeStrategy.Activate();
        }

        private void OnInventoryRequested(int tabIndex)
        {
            InventoryPresenter presenter = this.GameContext.GetPresenterManager()
                .GetPresenter<InventoryPresenter>(PresenterType.INV);

            if (tabIndex == -1)
            {
                presenter.Close();
                return;
            }

            if (!presenter.IsOpen())
                presenter.Open(this._activeStrategy.GetPlayer(), tabIndex);
            else if (presenter.GetActiveTabIndex() == tabIndex)
                presenter.Close();
            else
                presenter.NavigateToTab(tabIndex);
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