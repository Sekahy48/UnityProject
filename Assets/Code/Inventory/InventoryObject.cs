using System;
using System.Collections.Generic;
using ECS.Entity;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEditor;
using AC = Utils.ArgumentChecker;

namespace Inventory
{
    /// <summary>
    /// Representa un item o inventario (contenedor de items) que 
    /// puede ser contenido en otro inventario y contiene o puede contener
    /// otros items o inventarios.
    /// 
    /// Por ejemplo, el inventario del jugador, una mochila en el inventario 
    /// del jugador, o un bolsillo en la mochila.
    /// </summary>
    public class InventoryObject : IInventoryElement
    {
        /// Lista de elementos contenidos
        private List<IInventoryElement> _inventory;
        /// Identificador no unitario, representa el concepto del item contenedor (o inventario puro/root)
        private String _id;
        /// Entidad envuelta (si es root será null)
        private IEntity _item;

        /// Contructor de la clase
        public InventoryObject(IEntity item)
        {
            AC.CheckNotNull(item, item.GetCompoundIdentification().ToString());
            this._id = item.GetName();
            this._item = item;
            this._inventory = new List<IInventoryElement>();
        }

        /// <summary>
        /// Obtiene el identificador del inventario (no unitario)
        /// </summary>
        /// <returns>identificador del inventario</returns>
        public String GetId()
        {
            return this._id;
        }

        /// <summary>
        /// Obtiene la entidad item envuelta (si es root será null)
        /// </summary>
        /// <returns>entidad item envuelta</returns>
        public IEntity GetItemEntity()
        {
            return this._item;
        }

        /// Añade x cantidad de un item al inventario a la
        /// pila de dicho item que antes se encuentre en el arbol
        /// de inventario donde se esta añadiendo la cantidad.
        /// Si no existe dicho item en el arbol se añade como nuevo.
        /// Si el item a añadir no es hoja se añade directamente.
        public void AddAmountOfItem(IInventoryElement item, int amount)
        {
            AC.CheckNotNull(item, item.GetId());
            AC.CheckPositive(amount, "amount");
            IInventoryElement found = this.Contains(item.GetId());
            if (found != null)
            {
                if (!found.IsLeaf())
                {
                    this._inventory.Add(item);
                }
                else
                {
                    ItemObject leafItem = (ItemObject)found;
                    leafItem.ChangeAmount(amount);
                }
            }
            else
            {
                this._inventory.Add(item);
            }
        }

        /// Elimina x cantidad de un item
        /// <returns>si se ha podido eliminar</returns> 
        public bool RemoveAmountOfItem(IInventoryElement item, int amount)
        {
            return this.RetrieveAmountOfItem(item.GetId(), amount) != null;
        }

        /// Extrae una cantidad X de un item del inventario
        /// <returns>el item extraido o null si no se ha podido</returns>
        public IInventoryElement RetrieveAmountOfItem(String id, int amount)
        {
            AC.CheckNotNull(id, "id");
            AC.CheckPositive(amount, "amount");
            IInventoryElement found = this.Contains(id);
            if (found == null)
            {
                return null;
            }
            else
            {
                if (found.IsLeaf())
                {
                    ItemObject leafItem = (ItemObject)found;
                    int currentAmount = leafItem.GetAmount();
                    if (currentAmount <= amount)
                    {
                        this.Destroy(leafItem.GetId());
                        return leafItem;
                    }
                    else
                    {
                        leafItem.ChangeAmount(-amount);
                        return new ItemObject(leafItem.GetItemEntity(), amount);
                    }
                }
                else
                {
                    this.Destroy(found.GetId());
                    return found;
                }
            }
        }
        /// Obtiene la cantidad total de un item en el inventario
        /// teniendo en cuenta sub-inventarios
        /// <returns>cantidad total del item</returns>
        public int HowMuchThereIsOf(String id)
        {
            AC.CheckNotNull(id, "id");
            int total = 0;
            foreach (IInventoryElement elem in this._inventory)
            {
                if (elem.GetId().Equals(id) && elem.IsLeaf())
                {
                    ItemObject leafItem = (ItemObject)elem;
                    total += leafItem.GetAmount();
                }
                else if (!elem.IsLeaf())
                {
                    InventoryObject inv = (InventoryObject)elem;
                    total += inv.HowMuchThereIsOf(id);
                }
                
            }
            return total;
        }

        /// Comprueba si cierto item está en el inventario 
        /// y si lo esta lo devuelve
        /// <param name="id">identificador del item a buscar</param>
        /// <returns>si está o no</returns>
        public IInventoryElement Contains(String id)
        {
            AC.CheckNotNull(id, "id");
            IInventoryElement found = null;
            foreach (IInventoryElement elem in this._inventory)
            {
                found = elem.Contains(id);
                if (found != null) return found;
            }

            return found;
        }

        /// Destruye todas las unidades de un item del inventario
        /// <returns>si se ha podido destruir</returns>
        public Boolean Destroy(String id)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in this._inventory)
            {
                if (elem.GetId().Equals(id))
                {
                    this._inventory.Remove(elem);
                    return true;
                }
                else
                {
                    if (!elem.IsLeaf())
                    {
                        InventoryObject inv = (InventoryObject)elem;
                        return inv.Destroy(id);
                    }
                }
            }
            return false;
        }
        
        /// Comprueba si este objeto es una hoja 
        /// (esto es, si no es un inventario)
        /// <returns>si es hoja o no</returns>
        public Boolean IsLeaf()
        {
            return false;
        }
        
        public IInventoryElement Clone()
        {
            InventoryObject clone = new InventoryObject(this._item);
            foreach (IInventoryElement elem in this._inventory)
            {
                clone._inventory.Add(elem.Clone());
            }
            return clone;
        }
    }
}