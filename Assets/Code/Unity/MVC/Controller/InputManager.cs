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

        /// <summary>Unico dueno del bloqueo de la vista y del estado del cursor.</summary>
        private readonly Unity.LookControl _look = new Unity.LookControl();

        /// <summary>Si el inventario estaba abierto el fotograma anterior, para detectar el cierre.</summary>
        private bool _inventoryWasOpen;

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
                // Pasa por SetActiveStrategy como cualquier cambio de camara: antes se
                // asignaba a pelo y la camara de arranque no se suscribia a ningun evento.
                ICameraStrategy initial = _cameraRegister.GetActiveCamera();
                if (initial == null)
                {
                    Debug.LogError("No active camera strategy found in InputManager.");
                    return;
                }
                SetActiveStrategy(initial);
            }

            // Con el inventario abierto Alt no hace nada: la vista ya esta bloqueada por el
            // inventario, y cambiar el modo manual a escondidas sorprenderia al cerrarlo.
            if (Keyboard.current.leftAltKey.wasPressedThisFrame && !IsInventoryOpen())
                _look.Toggle(Unity.LookLockReason.Manual);

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                ICameraStrategy nextStrategy = _cameraRegister.NextCamera();
                SetActiveStrategy(nextStrategy);
            }

            _activeStrategy.Execute(deltaTime);

            UpdateWorldInteraction();
            UpdateLook();
        }

        /// <summary>
        /// Pone al dia los motivos de bloqueo de la vista que dependen de otro estado.
        /// Se pregunta cada fotograma por la misma razon que la interaccion con el mundo:
        /// el menu y el inventario se cierran por muchos caminos, y ninguno tiene que
        /// acordarse de avisar. LookControl solo toca el cursor cuando algo cambia.
        /// </summary>
        private void UpdateLook()
        {
            _look.SetAvatarCamera(_activeStrategy is Strategy.BaseCameraStrategy);

            bool inventoryOpen = IsInventoryOpen();
            _look.Set(Unity.LookLockReason.Inventory, inventoryOpen);

            // Cerrar el inventario (I, X o Esc) devuelve al juego: camara libre y cursor
            // oculto, aunque antes de abrirlo se hubiera pulsado Alt. Se detecta el cierre
            // (abierto -> cerrado) y no cada camino que cierra, por la misma razon que el
            // resto de motivos se sincronizan preguntando.
            if (_inventoryWasOpen && !inventoryOpen)
                _look.Set(Unity.LookLockReason.Manual, false);
            _inventoryWasOpen = inventoryOpen;

            WorldInteractionPresenter world = WorldPresenter();
            _look.Set(Unity.LookLockReason.RadialMenu, world != null && world.IsMenuOpen);
            _look.Set(Unity.LookLockReason.Inspect, world != null && world.IsInspecting);
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

        private bool IsInventoryOpen()
        {
            InventoryPresenter inventory = _presenterManager.GetPresenter<InventoryPresenter>(PresenterType.INV);
            return inventory != null && inventory.IsOpen();
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
                worldSource.OnInteractHoldStarted -= OnInteractHoldStarted;
                worldSource.OnInteractHoldReleased -= OnInteractHoldReleased;
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
                worldSource2.OnInteractTapped += OnInteractTapped;
                worldSource2.OnInteractHoldStarted += OnInteractHoldStarted;
                worldSource2.OnInteractHoldReleased += OnInteractHoldReleased;
            }

            if (_activeStrategy is Strategy.BaseCameraStrategy avatar)
                avatar.Look = _look;

            // Solo se cierra lo que esta abierto. Ademas de ser lo correcto, evita tocar la
            // vista antes de que termine de montarse: la camara de arranque pasa por aqui en
            // el primer fotograma, cuando la vista del inventario aun no tiene sus elementos.
            if (!(_activeStrategy is IInventoryInputSource))
            {
                InventoryPresenter presenter = _presenterManager
                    .GetPresenter<InventoryPresenter>(PresenterType.INV);
                if (presenter != null && presenter.IsOpen()) presenter.Close(false);
            }

            _activeStrategy.Activate();
        }

        /// <summary>
        /// El presentador ya sabe si esta atendiendo al mundo: con el inventario abierto
        /// esta cerrado y el toque no hace nada.
        /// </summary>
        private void OnInteractTapped() => WorldPresenter()?.OnTapped();

        private void OnInteractHoldStarted() => WorldPresenter()?.OpenMenu();

        private void OnInteractHoldReleased() => WorldPresenter()?.ReleaseMenu();

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
            // Esc tambien cierra el panel de inspeccion del mundo, si esta abierto.
            WorldPresenter()?.CloseInspect();

            InventoryPresenter presenter = _presenterManager
                .GetPresenter<InventoryPresenter>(PresenterType.INV);

            if (presenter != null && presenter.IsOpen()) presenter.Close(false);
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
