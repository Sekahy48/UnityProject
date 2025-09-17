using System;
using System.Collections.Generic;
using ECS.Entity;

namespace ECS.Component
{
    public class InventoryComponent : BasicComponent
    {
        private Dictionary<string, ItemEntity> items; // Mapa de ítems con su cantidad
        private int maxCapacity; // Capacidad máxima del inventario

        // Constructor
        public InventoryComponent(int maxCapacity)
        {
            items = new Dictionary<string, ItemEntity>();
            this.maxCapacity = maxCapacity;
            this.name = "InventoryComponent"; // Inicializa el nombre del componente
        }

        public override IComponent Clone()
        {
            InventoryComponent cloned = new InventoryComponent(this.maxCapacity);
            foreach (var kvp in this.items)
            {
                cloned.items[kvp.Key] = kvp.Value; // Copia las referencias de los ítems
            }
            return cloned;
        }

        // Métodos de acceso
        public int GetMaxCapacity()
        {
            return maxCapacity;
        }

        public void SetMaxCapacity(int maxCapacity)
        {
            this.maxCapacity = maxCapacity;
        }

        // Agregar un ítem al inventario
        public void AddItem(string itemName, ItemEntity item)
        {
            items[itemName] = item;
        }

        // Eliminar un ítem del inventario
        public bool RemoveItem(string itemName)
        {
            return items.Remove(itemName);
        }

        // Extrae X unidades del ítem
        public Entity.IEntity ExtractItem(string itemName, int quantity)
        {
            ItemEntity outItem = null;
            if (items.ContainsKey(itemName))
            {
                ItemEntity item = items[itemName];
                int currentAmount = (int)item.GetAmount();
                outItem = (ItemEntity)item.Clone();

                if (currentAmount <= quantity)
                {
                    this.RemoveItem(itemName);
                }
                else
                {
                    int remaining = currentAmount - quantity;
                    outItem.SetAmount(remaining);
                }
            }

            return outItem;
        }

        // Obtener la cantidad de un ítem en el inventario
        public int GetItemQuantity(string itemName)
        {
            return (int)items[itemName].GetAmount();
        }

        // Verificar si el inventario está lleno
        public bool IsFull()
        {
            return items.Count >= maxCapacity;
        }

        // Obtiene un ítem según una característica/capacidad
        public Entity.IEntity GetEntityByCapability(ItemCapability capability)
        {
            foreach (var elem in items.Values)
            {
                // TODO2: comprobar más cualidades si hace falta
                if (elem.HasCapability(capability))
                    return elem;
            }

            return null;
        }
    }
}
