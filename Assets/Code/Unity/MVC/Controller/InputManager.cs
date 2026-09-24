using Core.Contexts;
using Core.ECS.Entity;
using Core.MVC.Presenter;
using Core.MVC.Presenter.Inventory;
using Core.MVC.Presenter.World;
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

            UpdateWorldInteraction();
        }

        /// <summary>
        /// Abre o cierra la atencion al mundo y la hace avanzar un fotograma.
        ///
        /// Una sola regla para las dos condiciones: hay interaccion si la camara activa es
        /// fuente de interaccion (FPS, TPS; no RTS) y el inventario esta cerrado. Con el
        /// inventario abierto la E no hace nada en el mundo, este la camara bloqueada o no.
        /// Se evalua cada fotograma en vez de reaccionar a abrir/cerrar/cambiar de camara
        /// porque asi no hay ningun camino que se olvide de avisar, y porque tras una
        /// recarga de UI el presentador nuevo se pone al dia solo.
        /// </summary>
        private void UpdateWorldInteraction()
        {
            WorldInteractionPresenter world = WorldPresenter();
            if (world == null) return;

            InventoryPresenter inventory = _presenterManager.GetPresenter<InventoryPresenter>(PresenterType.INV);
            bool inventoryOpen = inventory != null && inventory.IsOpen();

            bool shouldAttend = _activeStrategy is IWorldInteractionInputSource && !inventoryOpen;

            if (shouldAttend && !world.IsOpen()) world.Open(_sessionContext._player);
            else if (!shouldAttend && world.IsOpen()) world.Close(false);

            world.Tick();
        }

        private WorldInteractionPresenter WorldPresenter()
            => _presenterManager.GetPresenter<WorldInteractionPresenter>(PresenterType.WORLD);

        public void SetActiveStrategy(ICameraStrategy strategy)
        {
            // Unsubscribe the previous one if it exists
            if (_activeStrategy != null && _activeStrategy is IInventoryInputSource invStrategy)
            {
                invStrategy.OnInventoryToggleRequested -= OnInventoryToggleRequested;
                invStrategy.OnInventoryCancelRequested -= OnInventoryCancelRequested; 
                invStrategy.OnInventoryPanelToggleRequested -= OnInventoryPanelToggleRequested;
            }
            if (_activeStrategy is IWorldInteractionInputSource worldSource)
            {
                worldSource.OnInteractTapped -= OnInteractTapped;
            }
            _activeStrategy = strategy;

            // Subscribe the new one
            if (_activeStrategy is IInventoryInputSource invStrategy2)
            {
                invStrategy2.OnInventoryToggleRequested += OnInventoryToggleRequested;
                invStrategy2.OnInventoryCancelRequested += OnInventoryCancelRequested; 
                invStrategy2.OnInventoryPanelToggleRequested += OnInventoryPanelToggleRequested;
            }
            if (_activeStrategy is IWorldInteractionInputSource worldSource2)
            {
                // Mantener y soltar tras mantener se conectan con el menu radial.
                worldSource2.OnInteractTapped += OnInteractTapped;
            }

            if (!(_activeStrategy is IInventoryInputSource))
            {
                InventoryPresenter presenter = _presenterManager
                    .GetPresenter<InventoryPresenter>(PresenterType.INV);
                presenter.Close(false);
            }

            _activeStrategy.Activate();
        }

        /// <summary>
        /// El presentador ya sabe si esta atendiendo al mundo: con el inventario abierto
        /// esta cerrado y el toque no hace nada.
        /// </summary>
        private void OnInteractTapped() => WorldPresenter()?.OnTapped();

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
