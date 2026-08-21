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
        public void Grab(InventoryObject inventory, ItemObject node, int amount, ItemEntity subLotItem = null)
        {
            AC.CheckNotNull(inventory, nameof(inventory));
            AC.CheckNotNull(node, nameof(node));
            AC.CheckPositive(amount, nameof(amount));

            _inventory = inventory;
            _node = node;
            _subLotItem = subLotItem;
            _grabbed = Math.Min(amount, node.GetAmount(subLotItem));
        }

        /// <summary>
        /// Removes units from the source node so they can be handed to a destination.
        /// This is the only point where anything actually moves — hence why cancelling
        /// costs nothing: until this runs, the items never left.
        ///
        /// Takes a positive count and negates it internally, so callers never reason about
        /// signs. Clamped to what is still held, and the hand clears itself once it runs out.
        /// </summary>
        /// <param name="amount">Units to place. More than is held places only what is held.</param>
        /// <returns>Units actually removed, which may be fewer than requested. Feed THIS to
        /// the destination, not the requested amount.</returns>
        public int PlaceAmount(int amount)
        {
            AC.CheckNotNull(_node, nameof(_node));
            AC.CheckPositive(amount, nameof(amount));

            int placed = _inventory.ModifyAmount(_node, _subLotItem, -Math.Min(amount, _grabbed));
            _grabbed -= placed;
            if (_grabbed <= 0) Clear();
            return placed;
        }

        /// <summary>
        /// Empties the hand. Safe as a cancel (ESC, closing the window): since the units
        /// never left their node, dropping the references undoes the grab entirely — there
        /// is nothing to give back.
        /// Also called automatically once everything held has been placed, which matters
        /// because the source node may have just been removed by CleanNode and the
        /// reference would be dangling.
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
    }
}