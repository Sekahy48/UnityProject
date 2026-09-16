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
        private bool _placedThisGesture;

        public GrabGesture(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>Pulsacion sobre esta superficie. Con la mano llena no agarra nada.</summary>
        public void OnPressed(Action grab)
        {
            _grabbedThisGesture = false;
            _placedThisGesture = false;
            if (_inventoryService.IsHandCarrying()) return;

            grab();
            _grabbedThisGesture = _inventoryService.IsHandCarrying();
        }

        /// <summary>
        /// Pulsacion que resuelve el gesto en el propio press: con la mano vacia agarra una
        /// porcion; con la mano llena agarra mas si se pulsa sobre su propio origen, y descarga
        /// una porcion en cualquier otro sitio.
        ///
        /// Al descargar se marca el gesto como resuelto, porque el release que viene detras
        /// colocaria el resto de la mano: para el gesto normal "mano llena + soltar" significa
        /// descargar, y aqui ya se descargo lo que se pedia. Rellenar cuenta como agarre, no
        /// como descarga, asi que deja el release en las mismas condiciones que un agarre.
        /// </summary>
        /// <param name="sameOrigin">La pulsacion cae sobre el origen de lo que ya se lleva. Lo
        /// decide quien llama: esta clase no conoce rejillas.</param>
        public void OnPortionPressed(bool sameOrigin, Action grabPortion, Action grabMore, Action placePortion)
        {
            _grabbedThisGesture = false;
            _placedThisGesture = false;

            if (!_inventoryService.IsHandCarrying())
            {
                grabPortion();
                _grabbedThisGesture = _inventoryService.IsHandCarrying();
                return;
            }

            if (sameOrigin)
            {
                grabMore();
                _grabbedThisGesture = true;
                return;
            }

            placePortion();
            _placedThisGesture = true;
        }

        public void OnReleased(bool dragged, Action place, Action cancel)
        {
            if (_placedThisGesture) return;
            if (!_inventoryService.IsHandCarrying()) return;
            if (_grabbedThisGesture && !dragged) return;

            place();

            if (dragged && _inventoryService.IsHandCarrying()) cancel();
        }
    }
}