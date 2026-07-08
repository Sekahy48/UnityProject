using System;

namespace ECS.Component
{
    public class ResourceComponent : BasicComponent
    {
        private ResourceType type;
        private int amount;
        private int maxAmount; // Max amount of resources
        private bool renewable;

        public ResourceComponent(ResourceType type, int amount, bool renewable)
        {
            this.type = type;
            this.amount = amount;
            this.maxAmount = amount;
            this.renewable = renewable;
            this._name = "ResourceComponent"; // Initializes the component name
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
                // Logic to increase the amount progressively
                // For now I'll keep it simple
                amount = Math.Min(amount + regeneratedAmount, maxAmount);
            }
            else
            {
                throw new NotSupportedException("Resource is not renewable");
            }
        }

        public override IComponent Clone()
        {
            return new ResourceComponent(this.type, this.amount, this.renewable); // Clones the component
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is ResourceComponent otherResource &&
                this.type == otherResource.type &&
                this.amount == otherResource.amount &&
                this.renewable == otherResource.renewable &&
                this.maxAmount == otherResource.maxAmount;
        }
    }
}
