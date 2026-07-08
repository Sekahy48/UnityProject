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
        /// Añade un nuevo nodo al inventario. Siempre crea nodo nuevo.
        /// Si la entidad tiene StorageComponent crea InventoryObject, si no ItemObject.
        /// </summary>
        void AddItem(ItemEntity item, int amount);

        /// <summary>
        /// Intenta apilar el item en el primer nodo equivalente encontrado.
        /// Si no encuentra ninguno, crea nodo nuevo.
        /// </summary>
        void StackOnto(ItemEntity item, int amount);

        /// <summary>
        /// Añade varios items como hojas nuevas.
        /// </summary>
        void AddSeveralItems(List<(ItemEntity item, int amount)> items);

        /// <summary>
        /// Modifica la cantidad de un item en el inventario global.
        /// </summary>
        int ModifyAmount(string id, int amount);

        /// <summary>
        /// Comprueba si el inventario contiene un item con el id dado.
        /// </summary>
        bool Contains(string id);

        /// <summary>
        /// Obtiene la cantidad total de un item en todo el inventario.
        /// </summary>
        int GetAmount(string id);

        /// <summary>
        /// Elimina todas las unidades de un item del inventario.
        /// </summary>
        void DeleteItem(string id);

        /// <summary>
        /// Devuelve el primer nodo que contiene el item con el id dado.
        /// </summary>
        IInventoryElement Find(string id);

        /// <summary>
        /// Devuelve todos los nodos que contienen el item con el id dado.
        /// </summary>
        List<IInventoryElement> FindNodes(string id);

        //#endregion

        //#region Local operations (solo nivel inmediato)

        void AddItemHere(ItemEntity item, int amount);
        void StackOntoHere(ItemEntity item, int amount);
        int ModifyAmountHere(string id, int amount);
        bool ContainsHere(string id);
        int GetAmountHere(string id);
        void DeleteItemHere(string id);
        IInventoryElement FindHere(string id);
        List<IInventoryElement> FindNodesHere(string id);

        //#endregion

        //#region Getters & Utilities

        string GetId();
        ItemEntity GetItemEntity();
        bool IsLeaf();
        int GetAmount();
        void SetAmount(int amount);
        void ClearInventory();
        void CleanTree();
        List<IInventoryElement> FlattenInventory();
        float GetTotalWeight();
        float GetTotalVolume();
        bool Equivalent(IInventoryElement other);
        IInventoryElement Clone();
        //#endregion
    }
}