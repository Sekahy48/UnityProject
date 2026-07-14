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
        /// Adds a new node to the inventory. Always creates a new node.
        /// If the entity has StorageComponent creates an InventoryObject, otherwise an ItemObject.
        /// </summary>
        void AddItem(ItemEntity item, int amount);

        /// <summary>
        /// Tries to stack the item onto the first equivalent node found.
        /// If none is found, creates a new node.
        /// </summary>
        void StackOnto(ItemEntity item, int amount);

        /// <summary>
        /// Adds several items as new leaves.
        /// </summary>
        void AddSeveralItems(List<(ItemEntity item, int amount)> items);

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

        void AddItemHere(ItemEntity item, int amount);
        void StackOntoHere(ItemEntity item, int amount);
        int ModifyAmountHere(int id, int amount);
        bool ContainsHere(int id);
        int GetAmountHere(int id);
        void DeleteItemHere(int id);
        IInventoryElement FindHere(int id);
        List<IInventoryElement> FindNodesHere(int id);

        //#endregion

        //#region Getters & Utilities

        int GetId();
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