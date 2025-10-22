using System;
using ECS.Entity;

namespace Inventory
{
    /// <summary>
    /// Representa un item (hoja) que puede ser contenido en un inventario.
    /// </summary>
    public class ItemObject : IInventoryElement
    {
         
        /// Identificador no unitario, representa el concepto del item (no la unidad física)
        private String _id;
        /// Entidad envuelta
        private IEntity _item;
        /// Cantidad de unidades de este item
        private int _amount;

        /// Contructor de la clase
        public ItemObject(IEntity item, int amount)
        {
            this._id = item.GetName();
            this._item = item;
            this._amount = amount;
        }

        /// Obtiene el identificador del item (no unitario)
        public String GetId()
        {
            return this._id;
        }

        /// Obtiene la entidad item envuelta
        public IEntity GetItemEntity()
        {
            return this._item;
        }

        /// Comprueba si contiene un item con el id dado
        public IInventoryElement Contains(String id)
        {
            if (this._id.Equals(id))
            {
                return this;
            }
            return null;
        }

        /// Comprueba si este objeto es una hoja 
        /// (esto es, si no es un inventario)   
        /// <returns>si es hoja o no</returns>
        public Boolean IsLeaf()
        {
            return true;
        }

        /// Obtiene la cantidad de unidades de este item
        /// <returns>cantidad de unidades</returns>
        public int GetAmount()
        {
            return this._amount;
        }

        /// Cambia la cantidad de unidades de este item
        public void ChangeAmount(int delta)
        {
            this._amount += delta;
            if (this._amount < 0) this._amount = 0;
        }

        /// Clona el item
        public IInventoryElement Clone()
        {
            return new ItemObject(this._item.Clone(), this._amount);
        }
        
    }
}