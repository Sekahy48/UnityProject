using System.Collections.Generic;

namespace ECS.Component
{
    public class FluidComponent : BasicComponent
    {
        private Dictionary<ResourceType, float> fluids;
        private float maxCapacity;

        public FluidComponent(float maxCapacity)
        {
            this.fluids = new Dictionary<ResourceType, float>();
            this.maxCapacity = maxCapacity;
            this.name = "FluidComponent";
        }

        public override IComponent Clone()
        {
            FluidComponent copy = new FluidComponent(this.maxCapacity);

            // Deep copy of the fluid map
            foreach (var entry in this.fluids)
            {
                copy.fluids[entry.Key] = entry.Value;
            }

            return copy;
        }

        // Add fluid to the component
        public float AddFluid(ResourceType fluid, float amount)
        {
            float left = this.GetSpaceLeft();
            float outAmount = -1;

            if (fluids.ContainsKey(fluid) && left > 0)
            {
                fluids[fluid] = System.Math.Min(amount, left);

                if (left < amount)
                {
                    outAmount = amount - left;
                }
                else
                {
                    outAmount = 0;
                }
            }

            return outAmount;
        }

        // Drain fluid from the component
        public float DrainFluid(ResourceType fluid, float amount)
        {
            float outAmount = 0;

            if (fluids.ContainsKey(fluid))
            {
                float left = fluids[fluid];
                if (amount >= left)
                {
                    fluids[fluid] = 0f;
                    outAmount = left;
                }
                else
                {
                    fluids[fluid] = left - amount;
                    outAmount = amount;
                }
            }
            return outAmount;
        }

        // Get remaining capacity
        public float GetSpaceLeft()
        {
            float content = 0;
            foreach (var amount in fluids.Values)
            {
                content += amount;
            }

            if (this.maxCapacity < content)
            {
                return -1;
            }

            return this.maxCapacity - content;
        }
    }
}
