using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;

namespace Inventory
{
    public class ItemObject : IInventoryElement
    {
        private int _id;
        private ItemEntity _item;
        private int _amount;

        public ItemObject(ItemEntity item, int amount)
        {
            this._id = item.GetComponent<BaseItemComponent>().GetTypeId();
            this._item = item;
            this._amount = Math.Max(0, amount);
        }

        public int GetId() => _id;
        public ItemEntity GetItemEntity() => _item;
        public bool IsLeaf() => true;
        public int GetAmount() => _amount;
        public void SetAmount(int amount) => _amount = Math.Max(0, amount);

        // Global and local operations are identical at a leaf
        public void AddItem(ItemEntity item, int amount) { }
        public void StackOnto(ItemEntity item, int amount) { }
        public void AddSeveralItems(List<(ItemEntity item, int amount)> items) { }
        public void AddItemHere(ItemEntity item, int amount) { }
        public void StackOntoHere(ItemEntity item, int amount) { }

        public int ModifyAmount(int id, int amount)
        {
            if (!this._id.Equals(id)) return 0;
            int before = _amount;
            _amount = Math.Max(0, _amount + amount);
            return Math.Abs(_amount - before);
        }

        public int ModifyAmountHere(int id, int amount) => ModifyAmount(id, amount);

        public bool Contains(int id) => this._id.Equals(id);
        public bool ContainsHere(int id) => Contains(id);

        public int GetAmount(int id) => this._id.Equals(id) ? _amount : 0;
        public int GetAmountHere(int id) => GetAmount(id);

        public void DeleteItem(int id) { if (this._id.Equals(id)) _amount = 0; }
        public void DeleteItemHere(int id) => DeleteItem(id);

        public IInventoryElement Find(int id) => this._id.Equals(id) ? this : null;
        public IInventoryElement FindHere(int id) => Find(id);

        public List<IInventoryElement> FindNodes(int id) =>
            this._id.Equals(id) ? new List<IInventoryElement> { this } : new List<IInventoryElement>();
        public List<IInventoryElement> FindNodesHere(int id) => FindNodes(id);

        public void ClearInventory() { }
        public void CleanTree() { }

        public List<IInventoryElement> FlattenInventory() => new List<IInventoryElement> { this };

        public float GetTotalWeight()
        {
            BaseItemComponent baseItem = _item.GetComponent<BaseItemComponent>();
            return baseItem.GetWeight() * _amount;
        } 

        public bool Equivalent(IInventoryElement other)
        {
            return other is ItemObject otherItem
                && this._id.Equals(otherItem._id)
                && this._amount == otherItem._amount
                && this._item.Equivalent(otherItem._item);
        }

        public IInventoryElement Clone() => new ItemObject(_item, _amount);
    }
}