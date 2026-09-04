using Core.Contexts;
using Core.ECS.Entity;
using Core.MVC.Presenter;
using Core.MVC.Presenter.Inventory;
using MVC.View.Inventory;
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
        private readonly CameraRegister _cameraRegister;
        private readonly PresenterManager _presenterManager;
        private readonly GameSessionContext _sessionContext;
        private ICameraStrategy _activeStrategy;

        public InputManager(CameraRegister cameraRegister, PresenterManager presenterManager, GameSessionContext sessionContext)
        {
            this._cameraRegister = cameraRegister;
            this._presenterManager = presenterManager;
            this._sessionContext = sessionContext;
        }

        public void Update(float deltaTime)
        {
            if (_activeStrategy == null)
            {
                _activeStrategy = _cameraRegister.GetActiveCamera();
                if (_activeStrategy == null)
                {
                    Debug.LogError("No active camera strategy found in InputManager.");
                    return;
                }
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                ICameraStrategy nextStrategy = _cameraRegister.NextCamera();
                SetActiveStrategy(nextStrategy);
            }

            _activeStrategy.Execute(deltaTime);
        }

        public void SetActiveStrategy(ICameraStrategy strategy)
        {
            // Unsubscribe the previous one if it exists
            if (_activeStrategy != null && _activeStrategy is IInventoryInputSource invStrategy)
            {
                invStrategy.OnInventoryToggleRequested -= OnInventoryToggleRequested;
                invStrategy.OnInventoryCancelRequested -= OnInventoryCancelRequested; 
                invStrategy.OnInventoryPanelToggleRequested -= OnInventoryPanelToggleRequested;
            }
            _activeStrategy = strategy;

            // Subscribe the new one
            if (_activeStrategy is IInventoryInputSource invStrategy2)
            {
                invStrategy2.OnInventoryToggleRequested += OnInventoryToggleRequested;
                invStrategy2.OnInventoryCancelRequested += OnInventoryCancelRequested; 
                invStrategy2.OnInventoryPanelToggleRequested += OnInventoryPanelToggleRequested;
            }
            else
            {
                InventoryPresenter presenter = _presenterManager
                    .GetPresenter<InventoryPresenter>(PresenterType.INV);
                presenter.Close(false);
            }

            _activeStrategy.Activate();
        }

        private void OnInventoryToggleRequested()
        {
            InventoryPresenter presenter = _presenterManager
                .GetPresenter<InventoryPresenter>(PresenterType.INV);

            if (presenter.IsOpen())
                presenter.Close();
            else
                presenter.Open(_sessionContext._player);
        }

        private void OnInventoryCancelRequested()
        {
            InventoryPresenter presenter = _presenterManager
                .GetPresenter<InventoryPresenter>(PresenterType.INV);

            presenter.Close(false);
        }

        private void OnInventoryPanelToggleRequested(PanelType panel)
        {
            InventoryPresenter presenter = _presenterManager
                .GetPresenter<InventoryPresenter>(PresenterType.INV);
            
            IEntity entity = panel == PanelType.A ? _sessionContext._firstInventorySrc : _sessionContext._secondInventorySrc;
            presenter.ToggleExtraInventory(entity, panel);
        }
    }
}
