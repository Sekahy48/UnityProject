using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using AC = Utils.ArgumentChecker;

namespace Inventory
{
    public interface IInventoryElement
    {
        //#region Global operations (recursive, BFS)

        /// <summary>
        /// Adds a new leaf node to this node's inventory.
        /// If the amount exceeds stacking limits, creates multiple nodes.
        /// </summary>
        /// <returns> The amount of item that couldnt be added due to internal limitations </returns>
        int  AddItem(ItemEntity item, int amount);

        /// <summary>
        /// Adds a new container node to this node's inventory.
        /// </summary>
        void AddContainer(ItemEntity item);

        /// <summary>
        /// Tries to stack the item onto the first equivalent node found.
        /// If none is found, creates a new node.
        /// </summary> 
        /// <returns> The amount of item that couldnt be added due to internal limitations </returns>
        int StackOnto(ItemEntity item, int amount);

        /// <summary>
        /// Modifies the amount of an item in the global inventory.
        /// </summary>
        int ModifyAmount(int id, int amount);

        /// <summary>
        /// Checks if the inventory contains an item with the given id.
        /// </summary>
        bool Contains(int id);

        /// <summary>
        /// Gets the total amount of an item across the whole inventory.
        /// </summary>
        int GetAmount(int id);

        /// <summary>
        /// Removes all units of an item from the inventory.
        /// </summary>
        void DeleteItem(int id);

        /// <summary>
        /// Returns the first node containing the item with the given id.
        /// </summary>
        IInventoryElement Find(int id);

        /// <summary>
        /// Returns all nodes containing the item with the given id.
        /// </summary>
        List<IInventoryElement> FindNodes(int id);

        //#endregion

        //#region Local operations (immediate level only)

        int StackOntoHere(ItemEntity item, int amount);
        int ModifyAmountHere(int id, int amount);
        bool ContainsHere(int id);
        int GetAmountHere(int id);
        void DeleteItemHere(int id);
        IInventoryElement FindHere(int id);
        List<IInventoryElement> FindNodesHere(int id);

        //#endregion

        //#region Node operations

        /// <summary>
        /// Stacks a certain amount of item in a preexistent node of the same item type.
        /// </summary>
        /// <param name="nodeId">Id of the tree node.</param>
        /// <param name="item">Item to add - needs to be the same type as the internal item of the node.</param>
        /// <param name="amount">Amount to be added</param>
        /// <returns> The remaining amount that couldnt be added. </returns>
        int StackOntoNode(int nodeId, ItemEntity item, int amount);
        
        /// <summary>
        /// Returns the ndoe with a certain nodeId if tis found. Otherwise returns null.
        /// </summary>
        /// <param name="nodeId">Id of the node to be found.</param>
        /// <returns>Found node or null.</returns>
        IInventoryElement FindNodeById(int nodeId);
        
        //#enregion

        //#region Getters & Utilities

        int GetTypeId();
        int GetNodeId();
        ItemEntity GetItemEntity();
        bool IsLeaf();
        int GetAmount();
        void SetAmount(int amount);
        void ClearInventory();
        void CleanTree();
        List<IInventoryElement> FlattenInventory();
        float GetTotalWeight(); 
        bool Equivalent(IInventoryElement other);
        IInventoryElement Clone();
        //#endregion
    }
}