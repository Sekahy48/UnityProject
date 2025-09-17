using System;
using System.Collections.Generic;
using ECS.Component;
using Handler; 

namespace ECS.Entity
{
    public class ItemEntity : InGameEntity
    {
        // Atributos comunes a cualquier item
        private int stackSize;       // Capacidad máxima del stack
        private float amount;        // Cantidad actual en este stack
        private float weight;        // Peso individual
        private float size;          // Tamaño (por ejemplo, en volumen)
        private float durability;    // Durabilidad actual
        private float maxDurability; // Durabilidad máxima

        private readonly List<ItemCapability> capabilities = new();

        /// <summary>
        /// Constructor de la clase ItemEntity
        /// </summary>
        public ItemEntity(int id, string type, int stackSize, float amount, float weight, float size, float durability, float maxDurability)
            : base(id, type)
        {
            this.stackSize = stackSize;
            this.amount = amount;
            this.weight = weight;
            this.size = size;
            this.durability = durability;
            this.maxDurability = maxDurability;
        }

        // ---- Getters ----

        public int GetStackSize() => stackSize;

        public float GetAmount() => amount;

        public float GetWeight() => weight;

        public float GetSize() => size;

        public float GetDurability() => durability;

        public float GetMaxDurability() => maxDurability;

        public float GetTotalWeight() => amount * weight;

        public int GetStacksNumber() => (int)Math.Ceiling(amount / stackSize);

        // ---- Setters ----

        public void SetStackSize(int stackSize) => this.stackSize = Math.Max(1, stackSize);

        public void SetAmount(float amount) => this.amount = Math.Max(0, amount);

        public void SetWeight(float weight) => this.weight = Math.Max(0f, weight);

        public void SetSize(float size) => this.size = Math.Max(0f, size);

        public void SetDurability(float durability) => this.durability = Math.Clamp(durability, 0f, maxDurability);

        public void SetMaxDurability(float maxDurability)
        {
            this.maxDurability = Math.Max(1f, maxDurability);
            if (this.durability > this.maxDurability)
                this.durability = this.maxDurability;
        }

        // ---- Modifiers (sumar/restar) ----

        public void ModifyAmount(float delta)
        {
            amount = Math.Clamp(amount + delta, 0, stackSize);
        }

        public void ModifyDurability(float delta)
        {
            durability = Math.Clamp(durability + delta, 0f, maxDurability);
        }

        public void ModifyWeight(float delta)
        {
            weight = Math.Max(0f, weight + delta);
        }

        public void ModifySize(float delta)
        {
            size = Math.Max(0f, size + delta);
        }

        public void ModifyStackSize(int delta)
        {
            stackSize = Math.Max(1, stackSize + delta);
        }

        public void ModifyMaxDurability(float delta)
        {
            maxDurability = Math.Max(1f, maxDurability + delta);
            if (durability > maxDurability)
                durability = maxDurability;
        }

        // ---- Booleans ----

        public bool IsBroken()
        {
             
            return capabilities.Contains(ItemCapability.BROKEN);
        }
  
        // ---- Capabilities ----

        // Método explícito para marcar el item como roto (añadir la capacidad)
        public void MarkAsBroken()
        {
            if (!capabilities.Contains(ItemCapability.BROKEN))
            {
                capabilities.Add(ItemCapability.BROKEN);
            }
        }
        
        // Método explícito para quitar la capacidad "roto"
        public void UnmarkBroken()
        {
            capabilities.Remove(ItemCapability.BROKEN);
        }
          
        public bool HasCapability(ItemCapability capability)
        {
            return capabilities.Contains(capability);
        }

        public bool AddCapability(ItemCapability capability)
        {
            if (capabilities.Contains(capability))
                return false;
            capabilities.Add(capability);
            return true;
        }

        public bool RemoveCapability(ItemCapability capability)
        {
            return capabilities.Remove(capability);
        }

        // ---- Components ----

        public float ModifyFluidContent(ResourceType fluid, float amount)
        {
            if (capabilities.Contains(ItemCapability.DRAINABLE))
            {
                var drainable = GetComponent<FluidComponent>(typeof(FluidComponent));
                if (drainable != null)
                {
                    return drainable.DrainFluid(fluid, amount);
                }
            }
            return -1;
        }
    }
}
