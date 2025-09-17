using System;

namespace ECS.Component
{
    public class ResourceComponent : BasicComponent
    {
        private ResourceType type;
        private int amount;
        private int maxAmount; // Cantidad máxima de recursos
        private bool renewable;

        public ResourceComponent(ResourceType type, int amount, bool renewable)
        {
            this.type = type;
            this.amount = amount;
            this.maxAmount = amount;
            this.renewable = renewable;
            this.name = "ResourceComponent"; // Inicializa el nombre del componente
        }

        public override IComponent Clone()
        {
            return new ResourceComponent(this.type, this.amount, this.renewable); // Clona el componente
        }

        public ResourceType GetResourceType()
        {
            return type;
        }

        public int GetAmount()
        {
            return amount;
        }

        public void DecreaseAmount(int value)
        {
            amount = Math.Max(0, amount - value);
        }

        public void IncreaseAmount(int value)
        {
            if (value > 0)
            {
                amount += value;
            }
            else
            {
                throw new ArgumentException("Value must be positive");
            }
        }

        public bool IsRenewable()
        {
            return renewable;
        }

        public void Regenerate(double percentage, double time)
        {
            if (renewable)
            {
                int regeneratedAmount = (int)(maxAmount * percentage);
                // Lógica de aumentar la cantidad progresivamente    
                // Por ahora lo dejo simple
                amount = Math.Min(amount + regeneratedAmount, maxAmount);
            }
            else
            {
                throw new NotSupportedException("Resource is not renewable");
            }
        }
    }
}
