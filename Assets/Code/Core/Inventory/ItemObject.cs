using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    /// <summary>
    /// Leaf node in the composite inventory tree.
    /// Wraps a BatchItem that holds sub-lots of equivalent items.
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

        //#region Getters

        public int GetTypeId() => _batch.GetTypeId();
        public int GetNodeId() => _nodeId;
        public ItemEntity GetItemEntity() => _batch.IsEmpty() ? null : _batch.GetSubLots()[0].Item;
        public BatchItem GetBatch() => _batch;
        public bool IsLeaf() => true;
        public int GetAmount() => _batch.GetTotalAmount();

        //#endregion

        //#region Leaf operations

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
            if (!this.GetTypeId().Equals(id)) return 0;

            if (amount > 0)
            {
                // Adding: use first sub-lot's entity as representative
                ItemEntity representative = GetItemEntity();
                int remaining = _batch.AddAmount(representative, amount);
                return amount - remaining;
            }
            else if (amount < 0)
            {
                // Consuming: consume from random sub-lots
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
        /// Modifies ONE specific sub-lot, identified by an equivalent item.
        /// Unlike ModifyAmount(int typeId, int) this never consumes at random: use it
        /// when the caller knows exactly which variant it is moving (the hand holding a
        /// specific sub-lot, for instance) rather than "any N units of this type".
        /// Empty sub-lots are dropped by BatchItem itself; emptying the whole node is
        /// the caller's problem — see InventoryObject.ModifyAmount(ItemObject, ItemEntity, int),
        /// which removes the node and frees its grid cells.
        /// </summary>
        /// <param name="item">Item identifying the sub-lot (matched by Equivalent).</param>
        /// <param name="amount">Positive adds, negative consumes.</param>
        /// <returns>Units actually applied, always positive.</returns>
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
        /// Takes units out and reports WHICH variants came out, grouped by sub-lot.
        ///
        /// ModifyAmount returns a bare count, which loses that information: a node holding
        /// 10 rusty and 10 pristine swords consumes at random, and the caller would have to
        /// guess what it just removed. Anything transferring items elsewhere needs the real
        /// breakdown, or the destination silently receives N copies of one variant and the
        /// others cease to exist.
        /// </summary>
        /// <param name="item">Sub-lot to take from, or null to take at random across the node.</param>
        /// <param name="amount">Units to take. Taking more than there is takes what there is.</param>
        /// <returns>Pairs of (variant, units taken). Empty if nothing came out.</returns>
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
        /// Units available in one sub-lot, or in the whole node when <paramref name="item"/>
        /// is null. Mirrors ModifyAmount(ItemEntity, int): same granularity, same null
        /// semantics, so a caller can ask "how much can I take?" and then take it without
        /// switching between two different APIs.
        /// </summary>
        /// <param name="item">Sub-lot to measure (matched by Equivalent), or null for the whole node.</param>
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
            if (this.GetTypeId().Equals(id))
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

        //#endregion

        //#region Utilities

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
            // Clone first sub-lot's entity to create a new ItemObject
            List<SubLot> subLots = _batch.GetSubLots();
            if (subLots.Count == 0) throw new InvalidOperationException("Cannot clone an ItemObject which BatchItem is empty. This node should be removed already.");

            ItemObject clone = new ItemObject(subLots[0].Item.Clone(), subLots[0].Amount);
            for (int i = 1; i < subLots.Count; i++)
            {
                clone._batch.AddAmount(subLots[i].Item.Clone(), subLots[i].Amount);
            }
            return clone;
        }

        //#endregion

        //#region nodes

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

        //#endregion

        //#region Transparent Composite — Not applicable to leaves

        public int AddItem(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("AddItem is not supported on leaf nodes.");
        }

        public void AddContainer(ItemEntity item)
        {
            throw new InvalidOperationException("AddContainer is not supported on leaf nodes.");
        }

        public int StackOnto(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("StackOnto is not supported on leaf nodes.");
        }

        



        //#endregion

    }
}
