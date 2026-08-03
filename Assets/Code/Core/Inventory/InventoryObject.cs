using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace Inventory
{
    public class InventoryObject : IInventoryElement
    {
        private const int BASE_GRID_W = 10;
        private const int BASE_GRID_H = 8;

        private List<IInventoryElement> _inventory;
        private TetrisGridState _grid;
        private int _id;
        private int _nodeId;
        private ItemEntity _item;

        public InventoryObject(ItemEntity item)
        {
            AC.CheckNotNull(item, item.GetCompoundIdentification().ToString());
            _id = item.GetComponent<BaseItemComponent>().GetTypeId();
            _nodeId = NodeIdGenerator.GenerateId();
            _item = item;
            _inventory = new List<IInventoryElement>();

            StorageComponent storage = item.GetComponent<StorageComponent>();
            _grid = new TetrisGridState(storage.GridH, storage.GridW);
        }

        public InventoryObject()
        {
            _id = 0;
            _nodeId = NodeIdGenerator.GenerateId();
            _item = null;
            _inventory = new List<IInventoryElement>();
            _grid = new TetrisGridState(BASE_GRID_H, BASE_GRID_W);
        }

        public int GetTypeId() => _id;
        public int GetNodeId() => _nodeId;
        public ItemEntity GetItemEntity() => _item;
        public bool IsLeaf() => false;
        public int GetAmount() => 1;
        public void SetAmount(int amount) { } // containers don't have an amount
        public List<IInventoryElement> GetChildren() => new List<IInventoryElement>(_inventory);
        public TetrisGridState GetGrid() => _grid;



        //#region BFS helper

        private IInventoryElement BfsFind(int id)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetTypeId().Equals(id))
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        private IInventoryElement BfsFindByNodeId(int nodeId)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetNodeId().Equals(nodeId))
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
                if (current.GetTypeId().Equals(id))
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
                if (current.IsLeaf() && current.GetTypeId() == item.GetComponent<BaseItemComponent>().GetTypeId())
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        //#endregion 

        //#region Global operations

        public int AddItem(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount"); 
            int remaining = 0;
            
            while (amount > 0 && remaining == 0)
            {   
                ItemObject newNode = new ItemObject(item, amount);
                if (_grid.TryFirstPlace(newNode))
                { 
                    amount = amount - newNode.GetAmount();
                    _inventory.Add(newNode); 
                } else
                {
                    remaining = amount;
                }
            } 

            return remaining;
        }

        public void AddContainer(ItemEntity item)
        {
            AC.CheckNotNull(item, "item");
            _inventory.Add(new InventoryObject(item)); 
        }

        public int StackOnto(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            IInventoryElement match = BfsFindEquivalent(item);
            if (match != null && match.IsLeaf()) 
                amount = match.StackOntoHere(item, amount); 

            if (amount > 0)
                amount = AddItem(item, amount); 
            return amount;
        }


        public int ModifyAmount(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            IInventoryElement found = BfsFind(id);
            if (found == null) return 0;
            int modified = found.ModifyAmount(id, amount);
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
            foreach (IInventoryElement node in BfsFindAll(id))
                node.DeleteItem(id);
            CleanTree();
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

        public int StackOntoHere(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.IsLeaf() && elem.GetTypeId() == item.GetComponent<BaseItemComponent>().GetTypeId())
                {
                    amount = elem.StackOntoHere(item, amount);
                    break;
                }
            }
            if (amount > 0)
                amount = AddItem(item, amount);
            return amount;
        }

        public int ModifyAmountHere(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.GetTypeId().Equals(id))
                {
                    int modified = elem.ModifyAmount(id, amount);
                    CleanTree();
                    return modified;
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
                if (elem.GetTypeId().Equals(id))
                    total += elem.GetAmount();
            return total;
        }

        public void DeleteItemHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> snapshot = new List<IInventoryElement>(_inventory);
            foreach (IInventoryElement elem in snapshot)
                if (elem.GetTypeId().Equals(id))
                    _inventory.Remove(elem);
        }

        public IInventoryElement FindHere(int id)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetTypeId().Equals(id)) return elem;
            return null;
        }

        public List<IInventoryElement> FindNodesHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> results = new List<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetTypeId().Equals(id)) results.Add(elem);
            return results;
        }

        //#endregion

        //#region Node operations

        public int StackOntoNode(int nodeId, ItemEntity item, int amount)
        {
            IInventoryElement node = FindNodeById(nodeId);
            return node != null? node.StackOntoHere(item, amount) : amount;
        }

        public IInventoryElement FindNodeById(int nodeId)
        {
            return BfsFindByNodeId(nodeId);
        }

        public int AddItemAt(ItemEntity item, int amount, int row, int col) 
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            AC.CheckPositive(row, "row");
            AC.CheckPositive(col, "col");

            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            if (_grid.CanPlace(row, col, baseInfo.GetDimensionH(), baseInfo.GetDimensionW())){
                ItemObject node = new ItemObject(item, amount);
                amount = amount - node.GetAmount();
                _grid.Place(node, row, col);
                _inventory.Add(node);
            }

            return amount;
        }

        //#endregion

        //#region Getters & Utilities

        public void ClearInventory() => _inventory.Clear();

        public void CleanTree()
        {
            foreach (IInventoryElement elem in _inventory)
                elem.CleanTree();

            _inventory.RemoveAll(e =>
            {
                if (e.GetAmount() <= 0)
                {
                    if (e.IsLeaf())
                        _grid.Remove(e.GetNodeId());
                    return true;
                }
                return false;
            });
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

            bool generalCheck = this._id.Equals(other.GetTypeId())
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

        public List<(ItemEntity, int)> ConsumeRandom(int amount)
        {
            throw new InvalidOperationException("AddContainer is not supported on leaf nodes."); 
        }

        //#endregion
    }
}