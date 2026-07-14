using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace Inventory
{
    public class InventoryObject : IInventoryElement
    {
        private List<IInventoryElement> _inventory;
        private int _id;
        private ItemEntity _item;

        public InventoryObject(ItemEntity item)
        {
            AC.CheckNotNull(item, item.GetCompoundIdentification().ToString());
            this._id = item.GetComponent<BaseItemComponent>().GetTypeId();
            this._item = item;
            this._inventory = new List<IInventoryElement>();
        }

        public InventoryObject()
        {
            this._id = 0;
            this._item = null;
            this._inventory = new List<IInventoryElement>();
        }

        public int GetId() => this._id;
        public ItemEntity GetItemEntity() => this._item;
        public bool IsLeaf() => false;
        public int GetAmount() => 1;
        public void SetAmount(int amount) { } // containers don't have an amount
        public List<IInventoryElement> GetChildren() => new List<IInventoryElement>(_inventory);

        //#region BFS helper

        private IInventoryElement BfsFind(int id)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetId().Equals(id))
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        private List<IInventoryElement> BfsFindAll(int id)
        {
            List<IInventoryElement> results = new List<IInventoryElement>();
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetId().Equals(id))
                    results.Add(current);
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return results;
        }

        private IInventoryElement BfsFindEquivalent(ItemEntity item)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.IsLeaf() && current.GetItemEntity().Equivalent(item))
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        //#endregion

        //#region Global operations

        public void AddItem(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            bool isContainer = item.HasComponent(typeof(StorageComponent));
            IInventoryElement node = isContainer
                ? (IInventoryElement)new InventoryObject(item)
                : new ItemObject(item, amount);
            _inventory.Add(node);
        }

        public void StackOnto(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            IInventoryElement match = BfsFindEquivalent(item);
            if (match != null)
                match.SetAmount(match.GetAmount() + amount);
            else
                AddItem(item, amount);
        }

        public void AddSeveralItems(List<(ItemEntity item, int amount)> items)
        {
            AC.CheckNotNull(items, "items");
            foreach (var (item, amount) in items)
                AddItem(item, amount);
        }

        public int ModifyAmount(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            IInventoryElement found = BfsFind(id);
            if (found == null) return 0;
            int before = found.GetAmount();
            found.SetAmount(Math.Max(0, found.GetAmount() + amount));
            int modified = Math.Abs(found.GetAmount() - before);
            CleanTree();
            return modified;
        }

        public bool Contains(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFind(id) != null;
        }

        public int GetAmount(int id)
        {
            AC.CheckNotNull(id, "id");
            int total = 0;
            foreach (IInventoryElement node in BfsFindAll(id))
                total += node.GetAmount();
            return total;
        }

        public void DeleteItem(int id)
        {
            AC.CheckNotNull(id, "id");
            ModifyAmount(id, -GetAmount(id));
        }

        public IInventoryElement Find(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFind(id);
        }

        public List<IInventoryElement> FindNodes(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFindAll(id);
        }

        //#endregion

        //#region Local operations

        public void AddItemHere(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            bool isContainer = item.HasComponent(typeof(StorageComponent));
            IInventoryElement node = isContainer
                ? (IInventoryElement)new InventoryObject(item)
                : new ItemObject(item, amount);
            _inventory.Add(node);
        }

        public void StackOntoHere(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.IsLeaf() && elem.GetItemEntity().Equivalent(item))
                {
                    elem.SetAmount(elem.GetAmount() + amount);
                    return;
                }
            }
            AddItemHere(item, amount);
        }

        public int ModifyAmountHere(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.GetId().Equals(id))
                {
                    int before = elem.GetAmount();
                    elem.SetAmount(Math.Max(0, elem.GetAmount() + amount));
                    CleanTree();
                    return Math.Abs(elem.GetAmount() - before);
                }
            }
            return 0;
        }

        public bool ContainsHere(int id)
        {
            AC.CheckNotNull(id, "id");
            return FindHere(id) != null;
        }

        public int GetAmountHere(int id)
        {
            AC.CheckNotNull(id, "id");
            int total = 0;
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetId().Equals(id))
                    total += elem.GetAmount();
            return total;
        }

        public void DeleteItemHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> snapshot = new List<IInventoryElement>(_inventory);
            foreach (IInventoryElement elem in snapshot)
                if (elem.GetId().Equals(id))
                    _inventory.Remove(elem);
        }

        public IInventoryElement FindHere(int id)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetId().Equals(id)) return elem;
            return null;
        }

        public List<IInventoryElement> FindNodesHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> results = new List<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetId().Equals(id)) results.Add(elem);
            return results;
        }

        //#endregion

        //#region Getters & Utilities

        public void ClearInventory() => _inventory.Clear();

        public void CleanTree()
        {
            foreach (IInventoryElement elem in _inventory)
                elem.CleanTree();
            _inventory.RemoveAll(e => e.GetAmount() <= 0);
        }

        public List<IInventoryElement> FlattenInventory()
        {
            List<IInventoryElement> result = new List<IInventoryElement>();
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.IsLeaf())
                    result.Add(current);
                else
                {
                    if (((InventoryObject)current)._inventory.Count > 0)
                        result.Add(current);
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
                }
            }
            return result;
        }

        public float GetTotalWeight()
        {
            float total = 0f;
            foreach (IInventoryElement elem in _inventory)
                total += elem.GetTotalWeight();
            return total;
        } 

        public bool Equivalent(IInventoryElement other)
        {
            AC.CheckNotNull(other, "other");
            if (!(other is InventoryObject otherInv)) return false;

            bool generalCheck = this._id.Equals(other.GetId())
                             && this.IsLeaf() == other.IsLeaf()
                             && (this._item == null ? otherInv._item == null
                                                    : this._item.Equivalent(otherInv._item));
            if (!generalCheck) return false;
            if (this._inventory.Count != otherInv._inventory.Count) return false;

            List<IInventoryElement> remaining = new List<IInventoryElement>(otherInv._inventory);
            foreach (IInventoryElement elem in _inventory)
            {
                IInventoryElement match = null;
                foreach (IInventoryElement candidate in remaining)
                {
                    if (elem.Equivalent(candidate)) { match = candidate; break; }
                }
                if (match == null) return false;
                remaining.Remove(match);
            }
            return remaining.Count == 0;
        }

        public IInventoryElement Clone()
        {
            InventoryObject clone = this._item != null
                ? new InventoryObject(this._item)
                : new InventoryObject();
            foreach (IInventoryElement elem in _inventory)
                clone._inventory.Add(elem.Clone());
            return clone;
        }

        //#endregion
    }
}