using System;
using ECS.Entity;

namespace Inventory
{
    /// <summary>
    /// Interfaz que representa un item o inventario (contenedor de items) que
    /// </summary>
    public interface IInventoryElement
    {
        public String GetId();

        public IEntity GetItemEntity();

        public IInventoryElement Contains(String id);

        public Boolean IsLeaf();

        public IInventoryElement Clone();
    }
}