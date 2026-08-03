using MVC.Presenter;
using MVC.Presenter.Inventory;
using Strategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MVC.Controller
{
    /// <summary>
    /// Manages player input. Receives only CameraRegister and PresenterManager,
    /// not the whole GameContext.
    /// </summary>
    public class InputManager
    {
        private readonly CameraRegister cameraRegister;
        private readonly PresenterManager presenterManager;
        private ICameraStrategy _activeStrategy;

        public InputManager(CameraRegister cameraRegister, PresenterManager presenterManager)
        {
            this.cameraRegister = cameraRegister;
            this.presenterManager = presenterManager;
        }

        public void Update(float deltaTime)
        {
            if (_activeStrategy == null)
            {
                _activeStrategy = cameraRegister.GetActiveCamera();
                if (_activeStrategy == null)
                {
                    Debug.LogError("No active camera strategy found in InputManager.");
                    return;
                }
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                ICameraStrategy nextStrategy = cameraRegister.NextCamera();
                SetActiveStrategy(nextStrategy);
            }

            _activeStrategy.Execute(deltaTime);
        }

        public void SetActiveStrategy(ICameraStrategy strategy)
        {
            // Unsubscribe the previous one if it exists
            if (_activeStrategy != null && _activeStrategy is IInventoryInputSource invStrategy)
                invStrategy.OnInventoryRequested -= OnInventoryRequested;

            _activeStrategy = strategy;

            // Subscribe the new one
            if (_activeStrategy is IInventoryInputSource invStrategy2)
            {
                invStrategy2.OnInventoryRequested += OnInventoryRequested;
            }
            else
            {
                InventoryPresenter presenter = presenterManager
                    .GetPresenter<InventoryPresenter>(PresenterType.INV);
                presenter.Close();
            }

            _activeStrategy.Activate();
        }

        private void OnInventoryRequested(int tabIndex)
        {
            InventoryPresenter presenter = presenterManager
                .GetPresenter<InventoryPresenter>(PresenterType.INV);

            if (tabIndex == -1 || presenter.IsOpen())
                presenter.Close();
            else
                presenter.Open(_activeStrategy.GetPlayer());
        }
    }
}
