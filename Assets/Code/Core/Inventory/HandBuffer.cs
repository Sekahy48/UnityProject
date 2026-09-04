using System;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    public class HandBuffer
    {
        private IGrabOrigin _origin;
        private ItemEntity _subLotItem;
        private int _grabbed;

        /// <summary>
        /// Marca unidades como agarradas. No mueve nada: siguen en su origen hasta que se
        /// colocan. La mano solo recuerda de donde salen y cuantas quedan pendientes.
        /// </summary>
        /// <param name="amount">Unidades a agarrar. Se acota a lo que el origen tenga.</param>
        /// <param name="subLotItem">Variante concreta, o null para el origen entero.</param>
        /// <returns>Unidades realmente agarradas.</returns>
        public int Grab(IGrabOrigin origin, int amount, ItemEntity subLotItem = null)
        {
            AC.CheckNotNull(origin, nameof(origin));
            AC.CheckPositive(amount, nameof(amount));

            // Sobrescribir un agarre abandonaria las unidades anteriores: nunca salieron de
            // su origen, pero sin referencia nadie puede colocarlas.
            if (!IsEmpty())
                throw new InvalidOperationException(
                    "Cannot grab with a full hand. Place or clear it first.");

            _origin = origin;
            _subLotItem = subLotItem;
            _grabbed = Math.Min(amount, origin.Available(subLotItem));

            return _grabbed;
        }

        /// <summary>
        /// Descuenta unidades que el llamante YA ha sacado del origen. Llamar con lo que
        /// realmente se movio, nunca con lo que se pidio.
        /// La mano se vacia sola al llegar a cero, porque el origen puede haber dejado de
        /// existir y la referencia quedaria colgando.
        /// </summary>
        public int NotifyPlaced(int amount)
        {
            if (amount <= 0) return 0;

            int placed = Math.Min(amount, _grabbed);
            _grabbed -= placed;
            if (_grabbed <= 0) Clear();
            return placed;
        }

        /// <summary>
        /// Vacia la mano. Seguro como cancelacion: lo que quede nunca salio de su origen.
        /// Solo cancela lo pendiente; lo ya reportado por NotifyPlaced esta colocado y ahi se queda.
        /// </summary>
        public void Clear()
        {
            _origin = null;
            _subLotItem = null;
            _grabbed = 0;
        }

        public bool IsEmpty() => _origin == null;

        public int GetHeldAmount() => _grabbed;

        /// <summary>Origen de lo agarrado. Null con la mano vacia.</summary>
        public IGrabOrigin GetOrigin() => _origin;

        /// <summary>Variante agarrada, o null si se agarro el origen entero.</summary>
        public ItemEntity GetHeldSubLot() => _subLotItem;

        /// <summary>Item con el que pintar el icono que sigue al cursor.</summary>
        public ItemEntity GetHeldItem() => IsEmpty() ? null : (_subLotItem ?? _origin.Representative);

        public IEntity GetEntity() => _origin?.Owner;
    }
}