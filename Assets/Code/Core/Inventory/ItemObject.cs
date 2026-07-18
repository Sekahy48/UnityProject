using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;

namespace Inventory
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
        public ItemEntity GetItemEntity() => _batch.IsEmpty() ? null : _batch.GetSubLots()[0].Item1;
        public BatchItem GetBatch() => _batch;
        public bool IsLeaf() => true;
        public int GetAmount() => _batch.GetTotalAmount();

        //#endregion

        //#region Leaf operations

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
            var subLots = _batch.GetSubLots();
            if (subLots.Count == 0) throw new InvalidOperationException("Cannot clone an ItemObject which BatchItem is empty. This node should be removed already.");

            ItemObject clone = new ItemObject(subLots[0].Item1.Clone(), subLots[0].Item2);
            for (int i = 1; i < subLots.Count; i++)
            {
                clone._batch.AddAmount(subLots[i].Item1.Clone(), subLots[i].Item2);
            }
            return clone;
        }

        //#endregion

        //#region Transparent Composite — Not applicable to leaves

        public void AddItem(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("AddItem is not supported on leaf nodes.");
        }

        public void AddContainer(ItemEntity item)
        {
            throw new InvalidOperationException("AddContainer is not supported on leaf nodes.");
        }

        public void StackOnto(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("StackOnto is not supported on leaf nodes.");
        }

        public void StackOntoHere(ItemEntity item, int amount)
        {
            throw new InvalidOperationException("StackOntoHere is not supported on leaf nodes.");
        }

        //#endregion
    }
}
