using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    /// <summary>
    /// Nodo hoja del arbol compuesto del inventario.
    /// Envuelve un BatchItem que guarda sublotes de items equivalentes.
    /// </summary>
    public class ItemObject : IInventoryElement
    { 
        private readonly int _nodeId;
        private readonly BatchItem _batch;

        public ItemObject(ItemEntity item, int amount)
        { 
            _nodeId = NodeIdGenerator.GenerateId();
            _batch = new BatchItem(item, amount);
        }

        #region Getters

        public int GetTypeId() => _batch.GetTypeId();
        public int GetNodeId() => _nodeId;
        public ItemEntity GetItemEntity() => _batch.IsEmpty() ? null : _batch.GetSubLots()[0].Item;
        public BatchItem GetBatch() => _batch;
        public bool IsLeaf() => true;

        /// <summary>Mas de un sub-lote: la pila mezcla estados distintos del mismo item.</summary>
        public bool HasVariants() => GetSubLots.Count > 1;
        public int GetAmount() => _batch.GetTotalAmount();

        public IReadOnlyList<SubLot> GetSubLots => _batch.GetSubLots();

        #endregion

        #region Leaf operations

        public int StackOntoHere(ItemEntity item, int amount)
        {
            return _batch.AddAmount(item, amount);
        }

        public void SetAmount(int amount)
        {
            throw new InvalidOperationException(
                "Cannot set absolute amount on ItemObject. Use GetBatch().AddAmount() or GetBatch().ConsumeAmount().");
        }

        public int ModifyAmount(int id, int amount)
        {
            if (!GetTypeId().Equals(id)) return 0;

            if (amount > 0)
            {
                // Al anadir: se usa como representante la entidad del primer sublote
                ItemEntity representative = GetItemEntity();
                int remaining = _batch.AddAmount(representative, amount);
                return amount - remaining;
            }
            else if (amount < 0)
            {
                // Al consumir: se consume de sublotes al azar
                int toConsume = Math.Min(Math.Abs(amount), _batch.GetTotalAmount());
                int consumed = 0;
                for (int i = 0; i < toConsume; i++)
                {
                    if (_batch.ConsumeRandom() != null) consumed++;
                }
                return consumed;
            }
            return 0;
        }

        /// <summary>
        /// Modifica UN sublote concreto, identificado por un item equivalente.
        /// A diferencia de ModifyAmount(int typeId, int), nunca consume al azar: se usa
        /// cuando el llamante sabe exactamente que variante mueve (la mano con un sublote
        /// concreto, por ejemplo) y no "N unidades cualesquiera de este tipo".
        /// Los sublotes vacios los quita el propio BatchItem; vaciar el nodo entero es
        /// cosa del llamante: ver InventoryObject.ModifyAmount(ItemObject, ItemEntity, int),
        /// que quita el nodo y libera sus celdas de la rejilla.
        /// </summary>
        /// <param name="item">Item que identifica el sublote (se compara con Equivalent).</param>
        /// <param name="amount">Positivo anade, negativo consume.</param>
        /// <returns>Unidades aplicadas de verdad, siempre positivas.</returns>
        public int ModifyAmount(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, nameof(item));

            // Un nodo solo contiene items de un typeId. Pedirle que modifique otro es un
            // error del llamante, no un caso legitimo: sin esta guarda, AddAmount devolveria
            // todo sin anadir y ConsumeAmount devolveria 0, indistinguible de "estaba vacio".
            int itemTypeId = item.GetComponent<BaseItemComponent>().TypeId;
            if (itemTypeId != GetTypeId())
                throw new InvalidOperationException(
                    $"ItemObject.ModifyAmount: item of typeId {itemTypeId} does not belong " +
                    $"to this node, which holds typeId {GetTypeId()}.");

            if (amount > 0)
            {
                int remaining = _batch.AddAmount(item, amount);
                return amount - remaining;
            }

            if (amount < 0)
                return _batch.ConsumeAmount(item, -amount);

            return 0;
        }

        /// <summary>
        /// Saca unidades y dice QUE variantes han salido, agrupadas por sublote.
        ///
        /// ModifyAmount devuelve solo una cuenta y pierde esa informacion: un nodo con
        /// 10 espadas oxidadas y 10 nuevas consume al azar, y el llamante tendria que
        /// adivinar que ha sacado. Todo lo que traslada items a otro sitio necesita el
        /// desglose real; si no, el destino recibe N copias de una variante y las demas
        /// desaparecen sin avisar.
        /// </summary>
        /// <param name="item">Sublote del que sacar, o null para sacar al azar de todo el nodo.</param>
        /// <param name="amount">Unidades a sacar. Si se pide mas de lo que hay, se saca lo que hay.</param>
        /// <returns>Pares (variante, unidades sacadas). Vacio si no ha salido nada.</returns>
        public List<SubLot> Extract(ItemEntity item, int amount)
        {
            AC.CheckPositive(amount, nameof(amount));

            List<SubLot> extracted = new List<SubLot>();

            if (item == null)
            {
                extracted.AddRange(_batch.ConsumeRandom(amount));
            }
            else
            {
                int taken = ModifyAmount(item, -amount);
                if (taken > 0) extracted.Add(new SubLot(item, taken));
            }

            return extracted;
        }

        /// <summary>
        /// Unidades disponibles en un sublote, o en todo el nodo si <paramref name="item"/>
        /// es null. Es el espejo de ModifyAmount(ItemEntity, int): misma granularidad y mismo
        /// significado de null, para poder preguntar "cuanto puedo sacar" y sacarlo sin
        /// cambiar de una API a otra.
        /// </summary>
        /// <param name="item">Sublote a medir (se compara con Equivalent), o null para todo el nodo.</param>
        public int GetAmount(ItemEntity item)
        {
            if (item == null) return _batch.GetTotalAmount();

            foreach (SubLot lot in _batch.GetSubLots())
                if (lot.Item.Equivalent(item)) return lot.Amount;

            return 0;
        }

        public int ModifyAmountHere(int id, int amount) => ModifyAmount(id, amount);

        public bool Contains(int id) => GetTypeId().Equals(id);
        public bool ContainsHere(int id) => Contains(id);

        public int GetAmount(int id) => GetTypeId().Equals(id) ? _batch.GetTotalAmount() : 0;
        public int GetAmountHere(int id) => GetAmount(id);

        public void DeleteItem(int id)
        {
            if (GetTypeId().Equals(id))
            {
                _batch.ConsumeAll();
            }
             
        }

        public void DeleteItemHere(int id) => DeleteItem(id);

        public IInventoryElement Find(int id) => GetTypeId().Equals(id) ? this : null;
        public IInventoryElement FindHere(int id) => Find(id);

        public List<IInventoryElement> FindNodes(int id) =>
            GetTypeId().Equals(id) ? new List<IInventoryElement> { this } : new List<IInventoryElement>();
        public List<IInventoryElement> FindNodesHere(int id) => FindNodes(id);

        #endregion

        #region Utilities

        public void ClearInventory() { }
        public void CleanTree() { }

        public List<IInventoryElement> FlattenInventory() => new List<IInventoryElement> { this };

        public float GetTotalWeight() => _batch.GetTotalWeight();

        public bool Equivalent(IInventoryElement other)
        {
            if (!(other is ItemObject otherItem)) return false;
            return GetTypeId().Equals(otherItem.GetTypeId());
        }

        public IInventoryElement Clone()
        {
            // Clona la entidad del primer sublote para crear un ItemObject nuevo
            List<SubLot> subLots = _batch.GetSubLots();
            if (subLots.Count == 0) throw new InvalidOperationException("Cannot clone an ItemObject which BatchItem is empty. This node should be removed already.");

            ItemObject clone = new ItemObject(subLots[0].Item.Clone(), subLots[0].Amount);
            for (int i = 1; i < subLots.Count; i++)
            {
                clone._batch.AddAmount(subLots[i].Item.Clone(), subLots[i].Amount);
            }
            return clone;
        }

        #endregion

        #region nodes

        public List<SubLot> ConsumeRandom(int amount)
        {
            return _batch.ConsumeRandom(amount);
        }

        public int StackOntoNode(int nodeId, ItemEntity item, int amount)
        {
            throw new InvalidOperationException("StackOnto is not supported on leaf nodes.");
        }

        public IInventoryElement FindNodeById(int nodeId)
        {
            return nodeId == _nodeId ? this : null;
        }

        #endregion

        #region Transparent Composite — Not applicable to leaves

        public int AddItem(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("AddItem is not supported on leaf nodes.");
        } 

        public int StackOnto(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("StackOnto is not supported on leaf nodes.");
        }

        



        #endregion

    }
}
