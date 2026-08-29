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
            _id = item.GetComponent<BaseItemComponent>().TypeId;
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
                if (current.IsLeaf() && current.GetTypeId() == item.GetComponent<BaseItemComponent>().TypeId)
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
            AC.CheckNotNull(item, nameof(item));
            AC.CheckPositive(amount, nameof(amount));

            while (amount > 0)
            {
                ItemObject newNode = new ItemObject(item, amount);
                if (!_grid.TryFirstPlace(newNode)) return amount;   // no cabe: devuelve lo que sobra

                _inventory.Add(newNode);
                amount -= newNode.GetAmount();
            }

            return 0;
        }

        /// <summary>
        /// Adds an already-built node to this inventory <b>without placing it on the grid</b>.
        /// Unlike AddItem, which builds its own nodes and needs free cells, this takes a node
        /// that already exists and only makes this inventory its owner.
        ///
        /// <para>Exists for staging containers: a node that has to belong to some inventory to
        /// be operated on (HandBuffer.Grab requires an owner, and the move transaction extracts
        /// the units through it) but is never rendered and never competes for space. Items spawned by
        /// the dev creative panel start here.</para>
        ///
        /// <para>Do NOT use it for the player inventory or a real container: a node outside the
        /// grid occupies no cells, is invisible to RenderGridItems, and CleanNode would find
        /// nothing to free. Grid space is the capacity system, and this bypasses it.</para>
        /// </summary>
        public void AddNode(ItemObject node)
        {
            AC.CheckNotNull(node, nameof(node));
            _inventory.Add(node);
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


        /// <summary>
        /// Modifies the first node found holding this typeId, consuming at random across
        /// its sub-lots. Use it for "remove N units of this item from wherever", e.g. a
        /// recipe consuming materials. When the exact node and variant are known, prefer
        /// the overload taking an ItemObject: it skips the BFS and the full CleanTree.
        ///
        /// Cleanup goes through CleanTree, not CleanNode: BfsFind searches the whole tree,
        /// so the node may live inside a nested container — and only that container can
        /// free the grid cells it sits on. CleanNode would find nothing and silently
        /// leave an empty node holding its cells.
        /// </summary>
        public int ModifyAmount(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            IInventoryElement found = BfsFind(id);
            if (found == null) return 0;

            int modified = found.ModifyAmount(id, amount);

            // Solo hay algo que limpiar si el nodo se ha vaciado. Consumir 3 de 20 no
            // justifica recorrer el arbol entero, que es el caso mas frecuente.
            if (found.GetAmount() <= 0) CleanTree();

            return modified;
        }

        /// <summary>
        /// Modifies a node held directly by this inventory, and removes the node if it
        /// ends up empty. Preferred over ModifyAmount(int, int) when the caller already
        /// knows which node it is operating on — no BFS, and no full CleanTree traversal.
        ///
        /// <paramref name="item"/> selects the granularity:
        /// pass it to target one specific sub-lot (moving the rusty swords, not the new
        /// ones), or leave it null to treat the node as a whole, spreading the change
        /// across its sub-lots at random — which is what "place one at a time from a mixed
        /// stack" means, and there is no meaningful way to choose.
        /// </summary>
        /// <param name="node">Node to modify. Must be a direct child of this inventory.</param>
        /// <param name="item">Sub-lot to target (matched by Equivalent), or null for the whole node.</param>
        /// <param name="amount">Positive adds, negative consumes.</param>
        /// <param name="clean">
        /// Whether to drop the node when it ends up empty. Pass false inside a transaction
        /// that may still put units back: cleaning would destroy the node and the rollback
        /// would have to recreate it at its old coordinates. The caller is then responsible
        /// for calling CleanNode once the operation settles.
        /// </param>
        /// <returns>Units actually applied, always positive.</returns>
        public int ModifyAmount(ItemObject node, ItemEntity item, int amount, bool clean = true)
        {
            AC.CheckNotNull(node, nameof(node));

            int modified = item != null
                ? node.ModifyAmount(item, amount)
                : node.ModifyAmount(node.GetTypeId(), amount);

            if (clean && node.GetAmount() <= 0)
                CleanNode(node);

            return modified;
        }

        /// <summary>
        /// Takes units out of a node held directly by this inventory, reporting which variants
        /// came out. Same granularity rules as ModifyAmount: pass an item to take from one
        /// sub-lot, or null to take at random across the node.
        /// </summary>
        /// <param name="clean">
        /// Whether to drop the node if it ends up empty. Pass false inside a transaction that
        /// may put units back — see InventorySystem.TryMoveItemTo.
        /// </param>
        /// <returns>Pairs of (variant, units taken). Feed THESE to the destination: the
        /// breakdown is the point, a bare total would collapse mixed stacks into one variant.</returns>
        public List<(ItemEntity item, int amount)> Extract(ItemObject node, ItemEntity item, int amount, bool clean = true)
        {
            AC.CheckNotNull(node, nameof(node));

            List<(ItemEntity item, int amount)> extracted = node.Extract(item, amount);

            if (clean && node.GetAmount() <= 0)
                CleanNode(node);

            return extracted;
        }

        /// <summary>
        /// Removes an empty node from this inventory and frees its grid cells.
        /// Targeted counterpart to CleanTree: use it when the emptied node is already
        /// known, instead of walking the whole tree to rediscover it.
        /// Only looks at direct children — a node inside a nested container must be
        /// cleaned by that container, which is the one owning the grid it sits on.
        /// </summary>
        /// <returns>True if the node was found and removed.</returns>
        public bool CleanNode(ItemObject node)
        {
            AC.CheckNotNull(node, nameof(node));

            if (!_inventory.Remove(node)) return false;

            _grid.Remove(node.GetNodeId());
            return true;
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
                if (elem.IsLeaf() && elem.GetTypeId() == item.GetComponent<BaseItemComponent>().TypeId)
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

        /// <summary>
        /// Puts units at a grid position, stacking onto whatever is already there when
        /// compatible, and creating a node otherwise.
        ///
        /// <para>The stacking branch is what makes "drop onto an existing pile" work, and it
        /// also lets several variants of a mixed stack land on the same spot: the first call
        /// creates the node, the rest merge into it. Creating a node per call would make every
        /// call after the first fail against the cells the first one just took.</para>
        ///
        /// <para>An occupied cell holding a different typeId rejects everything and returns the
        /// full amount: BatchItem.AddAmount refuses items that are not its type, so "drop onto
        /// something unrelated does nothing" falls out without a special case.</para>
        ///
        /// <para>Stacking skips CanPlace on purpose — the node is already placed and does not
        /// grow. Only maxStackSize limits it.</para>
        /// </summary>
        /// <returns>Units that could not be added.</returns>
        public int AddItemAt(ItemEntity item, int amount, int row, int col, int ignoreNodeId = -1)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            // Not CheckPositive: row 0 and column 0 are valid cells.
            if (!_grid.IsInside(row, col))
                throw new ArgumentOutOfRangeException(
                    $"AddItemAt: cell ({row}, {col}) is outside a {_grid.GetGridH()}x{_grid.GetGridW()} grid.");

            // Cualquier celda del nodo vale: soltar sobre el centro de una espada de 1x3
            // encuentra su nodo igual que soltar sobre su esquina de origen.
            //
            // El nodo ignorado no cuenta como ocupante: es el que se esta moviendo, sigue en
            // la grid porque nada sale de ella hasta soltar, y apilar sobre el devolveria las
            // unidades a su sitio original en vez de moverlas.
            GridElement occupant = _grid.GetElementAt(row, col);
            if (occupant != null && occupant.GetNode().GetNodeId() != ignoreNodeId)
                return occupant.GetNode().StackOntoHere(item, amount);

            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            if (_grid.CanPlace(row, col, baseInfo.DimensionH, baseInfo.DimensionW, ignoreNodeId))
            {
                ItemObject node = new ItemObject(item, amount);

                // Placing may still fail even after CanPlace said yes, so the result decides:
                // adding the node anyway would leave it inside the inventory but off the grid,
                // where nothing renders it and no cell points at it.
                if (!_grid.Place(node, row, col, ignoreNodeId)) return amount;

                amount = amount - node.GetAmount();
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

        /// <summary>
        /// Deep-clones the inventory, preserving each item's position in the grid.
        /// Cloning only the element list is not enough: the new InventoryObject starts
        /// with an empty TetrisGridState, so every placed node must be re-placed at its
        /// original coordinates or the clone would report its items as unplaced.
        /// </summary>
        public IInventoryElement Clone()
        {
            InventoryObject clone = this._item != null
                ? new InventoryObject(this._item)
                : new InventoryObject();

            // Placed items: clone and restore their (row, col).
            foreach (GridElement placed in _grid.GetElements())
            {
                ItemObject nodeClone = (ItemObject)placed.GetNode().Clone();
                if (!clone._grid.Place(nodeClone, placed.GetRow(), placed.GetCol()))
                    throw new InvalidOperationException(
                        $"InventoryObject.Clone: could not re-place node {placed.GetNode().GetNodeId()} " +
                        $"at ({placed.GetRow()}, {placed.GetCol()}) in the cloned grid.");

                clone._inventory.Add(nodeClone);
            }

            // TODO: nested containers do not occupy grid cells yet (AddContainer bypasses
            // the grid), so they live only in the element list. Cloned as-is for now.
            foreach (IInventoryElement elem in _inventory)
                if (!elem.IsLeaf())
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