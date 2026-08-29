using System;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace Inventory
{
    public class HandBuffer
    {
        private InventoryObject _inventory;
        private ItemObject _node;
        private ItemEntity _subLotItem;
        private IEntity _entity;

        private int _grabbed; 

        /// <summary>
        /// Marks units as picked up. Nothing is moved: the units stay in their node until
        /// they are actually placed somewhere. All the hand records is where they are and
        /// how many are pending.
        ///
        /// Every field is assigned, including a null subLotItem — a partial grab would
        /// leave the previous one behind and the next placement would hit the wrong sub-lot.
        /// </summary>
        /// <param name="inventory">Container owning the node. Needed to place: only it can
        /// remove the node and free its grid cells once emptied.</param>
        /// <param name="node">Node the units come from.</param>
        /// <param name="amount">Units to grab. Clamped to what is actually available.</param>
        /// <param name="subLotItem">Specific variant to grab, or null for the whole node
        /// (placements then spread across its sub-lots at random).</param>
        /// <returns>Units actually grabbed.</returns>
        public int Grab(IEntity entity, InventoryObject inventory, ItemObject node, int amount, ItemEntity subLotItem = null)
        {
            AC.CheckNotNull(inventory, nameof(inventory));
            AC.CheckNotNull(node, nameof(node));
            AC.CheckPositive(amount, nameof(amount));

            // Overwriting a grab would strand the previous units: they never left their node,
            // but with no reference left nobody can place them — and a node held in a staging
            // container nothing else points at is collected with its items inside.
            if (!IsEmpty())
                throw new InvalidOperationException(
                    "Cannot grab with a full hand. Place or clear it first.");

            _entity = entity;
            _inventory = inventory;
            _node = node;
            _subLotItem = subLotItem;
            _grabbed = Math.Min(amount, node.GetAmount(subLotItem));

            return _grabbed;
        }

        /// <summary>
        /// Discounts units the caller has ALREADY taken out of the source node. This does not
        /// move anything: the whole point of the hand is that it never touches an inventory —
        /// whoever runs the transaction (InventoryService, through InventorySystem) extracts,
        /// places, rolls back leftovers and then reports here how many actually travelled.
        ///
        /// <para>Call it with what really moved, never with what was requested: a destination
        /// that is full or overweight takes fewer units than asked, and the difference stays
        /// in the hand precisely because it never left its node.</para>
        ///
        /// <para>The hand clears itself once nothing is left. That matters because the source
        /// node may have just been dropped by CleanNode and the reference would dangle.</para>
        /// </summary>
        /// <param name="amount">Units that actually moved. Clamped to what is still held.</param>
        /// <returns>Units actually discounted.</returns>
        public int NotifyPlaced(int amount)
        {
            AC.CheckNotNull(_node, nameof(_node));

            if (amount <= 0) return 0;   // no se movio nada: la mano sigue igual

            int placed = Math.Min(amount, _grabbed);
            _grabbed -= placed;
            if (_grabbed <= 0) Clear();
            return placed;
        }

        /// <summary>
        /// Empties the hand. Safe as a cancel (ESC, closing the window): whatever is still
        /// held never left its node, so dropping the references undoes it entirely — there is
        /// nothing to give back.
        ///
        /// <para>It only cancels what remains. Units already reported through NotifyPlaced
        /// are in the destination and stay there: a partial placement is not a transaction
        /// waiting to be confirmed, it is one that already happened.</para>
        ///
        /// <para>Also called automatically once nothing is left, which matters because the
        /// source node may have just been dropped by CleanNode and the reference would
        /// dangle.</para>
        /// </summary>
        public void Clear()
        {
            _inventory = null;
            _node = null;
            _subLotItem = null;
            _grabbed = 0;
        }

        /// <summary>
        /// Whether the hand is holding anything. This is what decides what a click means:
        /// on an empty hand a click grabs, on a full one it places — so callers ask this
        /// before they care about amounts.
        /// Checks the node rather than the count: the node is the structural truth, and
        /// survives the count getting out of sync.
        /// </summary>
        public bool IsEmpty() => _node == null;

        /// <summary>
        /// Units currently held. Note these have NOT left their source node: the hand is a
        /// reference, not a container, so this is how many units are pending placement, not
        /// how many exist somewhere else.
        /// </summary>
        public int GetHeldAmount() => _grabbed;

        /// <summary>
        /// Representative item of what is held, for the UI to draw the icon following the
        /// cursor. With a sub-lot grabbed it is that exact variant; with the whole node it
        /// falls back to the node's first sub-lot, since a mixed stack has no single item.
        /// Null when the hand is empty.
        /// </summary>
        public ItemEntity GetHeldItem()
        {
            if (IsEmpty()) return null;
            return _subLotItem ?? _node.GetItemEntity();
        }

        /// <summary>Node the held units are still sitting in, so the UI can dim it.</summary>
        public ItemObject GetSourceNode() => _node;

        /// <summary>
        /// Container owning the source node. Needed to run the move: it is the only one that
        /// can extract from the node and free its grid cells, and comparing it against the
        /// destination is what tells a reorder inside one inventory from a real transfer.
        /// </summary>
        public InventoryObject GetSourceInventory() => _inventory;

        /// <summary>Sub-lot variant being held, or null when the whole node was grabbed.</summary>
        public ItemEntity GetHeldSubLot() => _subLotItem;

        public IEntity GetEntity() => _entity;
    }
}