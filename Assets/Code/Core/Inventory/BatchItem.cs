using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    /// <summary>
    /// Agrupa items del mismo typeId en sublotes que se distinguen por su estado (durabilidad, condicion, etc.).
    /// Cada sublote es un par (ItemEntity, cantidad). Los items de un mismo sublote son Equivalent.
    /// La cantidad total de todos los sublotes no puede pasar de maxStackSize.
    ///
    /// <para>
    /// <b>Invariante: las entidades de los sublotes son inmutables mientras estan aqui.</b>
    /// Una misma instancia de <see cref="ItemEntity"/> puede estar compartida por varios sublotes, varios
    /// BatchItem y varios ItemObject en distintas posiciones de la rejilla: al partir un lote
    /// (porque supera maxStackSize o porque se mueve una parte) se copia la referencia, no el
    /// objeto. Modificar una entidad obtenida de <see cref="GetSubLots"/> cambiaria sin avisar
    /// todas las pilas que la comparten.
    /// </para>
    /// <para>
    /// Para cambiar el estado de unas unidades (desgaste, que se estropeen, un encantamiento...), copiar al escribir:
    /// clonar la entidad, restar esa cantidad del sublote original, aplicar el cambio al
    /// clon y anadirlo como sublote nuevo. Como el clon ya no es
    /// <c>Equivalent</c> al original, acaba en su propio sublote, que es justo la
    /// distincion que los sublotes existen para representar.
    /// </para>
    /// </summary>
    public class BatchItem
    {
        private Random _random;
        private readonly List<SubLot> _items;
        private readonly int _typeId;
        private readonly int _maxStackSize;

        public BatchItem(ItemEntity item, int amount)
        {
            _typeId = item.GetComponent<BaseItemComponent>().TypeId;
            _maxStackSize = item.GetComponent<BaseItemComponent>().MaxStackSize;
            AC.CheckPositive(_maxStackSize, "maxStackSize");

            _items = new List<SubLot>
            {
                new SubLot(item, Math.Min(amount, _maxStackSize))
            };
 
            _random = new Random();
        }

        // #region Lecturas

        /// <summary>
        /// TypeId comun a todos los items de esta pila.
        /// </summary>
        public int GetTypeId() => _typeId;

        /// <summary>
        /// Cantidad total maxima que admite la pila sumando todos los sublotes.
        /// </summary>
        public int GetMaxStackSize() => _maxStackSize;

        /// <summary>
        /// Suma de las cantidades de todos los sublotes.
        /// </summary>
        public int GetTotalAmount()
        {
            int total = 0;
            foreach (SubLot lot in _items)
            {
                total += lot.Amount;
            }
            return total;
        }

        /// <summary>
        /// Suma de (peso * cantidad) de todos los sublotes.
        /// </summary>
        public float GetTotalWeight()
        {
            float totalWeight = 0;
            foreach (SubLot lot in _items)
            {
                totalWeight += lot.TotalWeight;
            }
            return totalWeight;
        }

        /// <summary>
        /// Si la pila no tiene sublotes (se consumio todo).
        /// </summary>
        public bool IsEmpty() => _items.Count == 0;

        /// <summary>
        /// Devuelve una copia de la lista de sublotes para consultarla.
        /// </summary>
        public List<SubLot> GetSubLots() => new List<SubLot>(_items);

        // #endregion

        // #region Anadir

        /// <summary>
        /// Anade una cantidad de un item si coincide con el typeId de la pila.
        /// Si el item es Equivalent a un sublote existente, se suma a el;
        /// si no, se crea un sublote nuevo.
        /// </summary>
        /// <param name="item">Item a anadir (tiene que compartir el typeId de la pila).</param>
        /// <param name="amount">Unidades a anadir.</param>
        /// <returns>Cantidad que no se pudo anadir por el limite de maxStackSize.</returns>
        public int AddAmount(ItemEntity item, int amount)
        {
            if (item.GetComponent<BaseItemComponent>().TypeId != _typeId) return amount;

            int toAdd = Math.Min(amount, _maxStackSize - GetTotalAmount());
            int remaining = amount - toAdd;

            if (toAdd > 0 && !SetAmount(item, toAdd + GetBatchAmount(item)))
            {
                _items.Add(new SubLot(item, toAdd));
            }

            return remaining;
        }

        // #endregion

        // #region Consumir

        /// <summary>
        /// Consume 1 unidad de un sublote al azar.
        /// </summary>
        /// <returns>La ItemEntity consumida, o null si la pila esta vacia.</returns>
        public ItemEntity ConsumeRandom()
        {
            if (_items.Count == 0) return null;

            int rndmIndex = _random.Next(0, _items.Count);
            ItemEntity item = _items[rndmIndex].Item;
            bool consumed = ConsumeAmount(item, 1) != 0;
            return consumed ? item : null;
        }

        /// <summary>
        /// Consume N unidades al azar entre los sublotes. Devuelve lo consumido agrupado por sublote.
        /// </summary>
        /// <param name="amount">Unidades a consumir.</param>
        /// <returns>Lo consumido de cada sublote.</returns>
        public List<SubLot> ConsumeRandom(int amount)
        {
            List<SubLot> consumed = new List<SubLot>();
            int toConsume = Math.Min(amount, GetTotalAmount());

            for (int i = 0; i < toConsume; i++)
            {
                if (_items.Count == 0) break;
                int rndmIndex = _random.Next(0, _items.Count);
                ItemEntity item = _items[rndmIndex].Item;
                ConsumeAmount(item, 1);

                bool found = false;
                for (int j = 0; j < consumed.Count; j++)
                {
                    if (consumed[j].Item.Equivalent(item))
                    {
                        consumed[j] = new SubLot(consumed[j].Item, consumed[j].Amount + 1);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    consumed.Add(new SubLot(item, 1));
            }

            return consumed;
        }

        /// <summary>
        /// Consume como mucho la cantidad pedida del sublote que coincide con el item dado.
        /// </summary>
        /// <param name="item">Item que identifica el sublote (se compara con Equivalent).</param>
        /// <param name="amount">Unidades maximas a consumir.</param>
        /// <returns>Cantidad consumida de verdad.</returns>
        public int ConsumeAmount(ItemEntity item, int amount)
        {
            int consumed = Math.Min(amount, GetBatchAmount(item));
            bool applied = SetAmount(item, GetBatchAmount(item) - consumed);
            return applied ? consumed : 0;
        }


        public void ConsumeAll()
        {
            _items.Clear();
        }
        // #endregion

        /// <summary>
        /// Si la pila ya guarda una variante Equivalent a ese item. Sirve para apilar primero
        /// donde ya estan sus iguales y no repartir una misma variante entre varias pilas.
        /// </summary>
        public bool HasVariant(ItemEntity item) => GetBatchAmount(item) > 0;

        // #region Ayudantes privados

        /// <summary>
        /// Cantidad guardada en el sublote cuyo item es Equivalent al dado.
        /// </summary>
        private int GetBatchAmount(ItemEntity item)
        {
            foreach (SubLot lot in _items)
            {
                if (lot.Item.Equivalent(item)) return lot.Amount;
            }
            return 0;
        }

        /// <summary>
        /// Fija la cantidad del sublote que coincide con el item dado.
        /// Si llega a 0, el sublote se quita.
        /// </summary>
        /// <returns>True si se encontro un sublote que coincide.</returns>
        private bool SetAmount(ItemEntity item, int amount)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                SubLot lot = _items[i];
                if (item.Equivalent(lot.Item))
                {
                    if (amount <= 0)
                    {
                        _items.RemoveAt(i);
                    }
                    else
                    {
                        _items[i] = new SubLot(lot.Item, amount);
                    }
                    return true;
                }
            }
            return false;
        }

        // #endregion
    }
}
