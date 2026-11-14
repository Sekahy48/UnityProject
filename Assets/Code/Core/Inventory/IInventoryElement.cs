using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    public interface IInventoryElement
    {
        // #region Operaciones globales (recursivas, BFS)

        /// <summary>
        /// Anade un nodo hoja nuevo al inventario de este nodo.
        /// Si la cantidad supera los limites de apilado, crea varios nodos.
        /// </summary>
        /// <returns> La cantidad que no se pudo anadir por limites internos </returns>
        int  AddItem(ItemEntity item, int amount);

         

        /// <summary>
        /// Intenta apilar el item en el primer nodo equivalente que encuentre.
        /// Si no hay ninguno, crea un nodo nuevo.
        /// </summary> 
        /// <returns> La cantidad que no se pudo anadir por limites internos </returns>
        int StackOnto(ItemEntity item, int amount);

        /// <summary>
        /// Modifica la cantidad de un item en todo el inventario.
        /// </summary>
        int ModifyAmount(int id, int amount); 

         
        /// <summary>
        /// Si el inventario contiene un item con ese id.
        /// </summary>
        bool Contains(int id);

        /// <summary>
        /// Cantidad total de un item en todo el inventario.
        /// </summary>
        int GetAmount(int id);

        /// <summary>
        /// Cantidad total de items equivalentes a la variante dada.
        /// </summary>
        /// <param name="variant">Variante con la que comparar.</param>
        /// <returns>Unidades equivalentes en total.</returns>
        int GetAmount(ItemEntity variant);

        /// <summary>
        /// Quita del inventario todas las unidades de un item.
        /// </summary>
        void DeleteItem(int id);

        /// <summary>
        /// Primer nodo que contiene el item con ese id.
        /// </summary>
        IInventoryElement Find(int id);

        /// <summary>
        /// Todos los nodos que contienen el item con ese id.
        /// </summary>
        List<IInventoryElement> FindNodes(int id);

        // #endregion

        // #region Operaciones locales (solo el nivel inmediato)

        int StackOntoHere(ItemEntity item, int amount);
        int ModifyAmountHere(int id, int amount);
        bool ContainsHere(int id);
        int GetAmountHere(int id);
        void DeleteItemHere(int id);
        IInventoryElement FindHere(int id);
        List<IInventoryElement> FindNodesHere(int id);

        // #endregion

        // #region Operaciones de nodo

        /// <summary>
        /// Apila una cantidad de item en un nodo ya existente del mismo tipo de item.
        /// </summary>
        /// <param name="nodeId">Id del nodo del arbol.</param>
        /// <param name="item">Item a anadir: tiene que ser del mismo tipo que el del nodo.</param>
        /// <param name="amount">Cantidad a anadir</param>
        /// <returns> La cantidad que no se pudo anadir. </returns>
        int StackOntoNode(int nodeId, ItemEntity item, int amount);
        
        /// <summary>
        /// Devuelve el nodo con ese nodeId si lo encuentra; si no, null.
        /// </summary>
        /// <param name="nodeId">Id del nodo a buscar.</param>
        /// <returns>El nodo encontrado o null.</returns>
        IInventoryElement FindNodeById(int nodeId);
        
        /// <summary>
        /// Consume N unidades al azar entre los sublotes. Devuelve lo consumido agrupado por sublote.
        /// Solo para nodos hoja.
        /// </summary>
        List<SubLot> ConsumeRandom(int amount);

        /// <summary>
        /// Saca unidades de ESTE nodo. No toca al padre: retirar el nodo si se queda vacio es
        /// asunto de quien lo contiene, no suyo.
        ///
        /// <para>Una rama no se divide: o sale entera o no sale nada. Y su contenido viaja
        /// dentro de la entidad devuelta, en su InventoryComponent, asi que no necesita ningun
        /// caso aparte para llevarselo.</para>
        /// </summary>
        /// <param name="variant">Sub-lote del que sacar, o null para el nodo entero.</param>
        /// <returns>Pares (variante, unidades sacadas). Vacio si no salio nada.</returns>
        List<SubLot> Extract(ItemEntity variant, int amount);

        // #endregion

        // #region Lecturas y utilidades

        int GetTypeId();
        int GetNodeId();
        ItemEntity GetItemEntity();
        bool IsLeaf();

        /// <summary>
        /// Si lo que hay aqui son varias variantes del mismo item, con estados distintos.
        /// Una rama nunca: un contenedor no es una pila.
        /// </summary>
        bool HasVariants();
        int GetAmount();
        void SetAmount(int amount);
        void ClearInventory();
        void CleanTree();
        List<IInventoryElement> FlattenInventory();
        float GetTotalWeight(); 
        bool Equivalent(IInventoryElement other);
        IInventoryElement Clone();
        // #endregion
    }
}