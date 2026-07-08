using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;

namespace Inventory
{
    public class ItemObject : IInventoryElement
    {
        private string _id;
        private ItemEntity _item;
        private int _amount;

        public ItemObject(ItemEntity item, int amount)
        {
            this._id = item.GetName();
            this._item = item;
            this._amount = Math.Max(0, amount);
        }

        public string GetId() => _id;
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

        public int ModifyAmount(string id, int amount)
        {
            if (!this._id.Equals(id)) return 0;
            int before = _amount;
            _amount = Math.Max(0, _amount + amount);
            return Math.Abs(_amount - before);
        }

        public int ModifyAmountHere(string id, int amount) => ModifyAmount(id, amount);

        public bool Contains(string id) => this._id.Equals(id);
        public bool ContainsHere(string id) => Contains(id);

        public int GetAmount(string id) => this._id.Equals(id) ? _amount : 0;
        public int GetAmountHere(string id) => GetAmount(id);

        public void DeleteItem(string id) { if (this._id.Equals(id)) _amount = 0; }
        public void DeleteItemHere(string id) => DeleteItem(id);

        public IInventoryElement Find(string id) => this._id.Equals(id) ? this : null;
        public IInventoryElement FindHere(string id) => Find(id);

        public List<IInventoryElement> FindNodes(string id) =>
            this._id.Equals(id) ? new List<IInventoryElement> { this } : new List<IInventoryElement>();
        public List<IInventoryElement> FindNodesHere(string id) => FindNodes(id);

        public void ClearInventory() { }
        public void CleanTree() { }

        public List<IInventoryElement> FlattenInventory() => new List<IInventoryElement> { this };

        public float GetTotalWeight()
        {
            BaseItemComponent baseItem = _item.GetComponent<BaseItemComponent>();
            return baseItem.GetWeight() * _amount;
        }

        public float GetTotalVolume()
        {
            BaseItemComponent baseItem = _item.GetComponent<BaseItemComponent>();
            return baseItem.GetVolume() * _amount;
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