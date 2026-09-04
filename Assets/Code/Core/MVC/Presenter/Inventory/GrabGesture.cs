using System;
using Core.Services;

namespace Core.MVC.Presenter.Inventory
{
    /// <summary>
    /// Maquina de gestos de agarrar y colocar, compartida por cualquier superficie que
    /// pueda originar o recibir un agarre (rejilla de inventario, slot de equipo).
    /// No sabe de donde sale ni donde va: recibe esas dos acciones y se limita a decidir
    /// cuando ejecutarlas.
    /// Una instancia por superficie: el flag significa "este gesto se origino AQUI".
    /// </summary>
    public class GrabGesture
    {
        private readonly InventoryService _inventoryService;
        private bool _grabbedThisGesture;

        public GrabGesture(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>Pulsacion sobre esta superficie. Con la mano llena no agarra nada.</summary>
        public void OnPressed(Action grab)
        {
            _grabbedThisGesture = false;
            if (_inventoryService.IsHandCarrying()) { CoreLogger.Instance.Log("PRESS: mano llena, no agarro"); return; }

            grab();
            _grabbedThisGesture = _inventoryService.IsHandCarrying();
            CoreLogger.Instance.Log($"PRESS: agarrado={_grabbedThisGesture}");
        }

        public void OnReleased(bool dragged, Action place, Action cancel)
        {
            CoreLogger.Instance.Log($"UP: carrying={_inventoryService.IsHandCarrying()}, esteGesto={_grabbedThisGesture}, dragged={dragged}");
            if (!_inventoryService.IsHandCarrying()) return;
            if (_grabbedThisGesture && !dragged) return;

            place();
            CoreLogger.Instance.Log($"UP: tras place, carrying={_inventoryService.IsHandCarrying()}");

            if (dragged && _inventoryService.IsHandCarrying()) cancel();
        }
    }
}