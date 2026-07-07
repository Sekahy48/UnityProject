using System;
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
            this._name = "FluidComponent";
        }

        // Add fluid to the component
        public float AddFluid(ResourceType fluid, float amount)
        {
            float left = this.GetSpaceLeft();

            if (left <= 0)
                return -1;

            float toAdd = System.Math.Min(amount, left);

            if (fluids.ContainsKey(fluid))
                fluids[fluid] += toAdd;
            else
                fluids[fluid] = toAdd;

            return amount - toAdd;
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

        /// <summary>
        /// Determines if this component has the exact same fluid content as another
        /// fluid component, regardless of the order of the fluids.  
        /// </summary>
        /// <param name="other"> Another fluid component to compare with.</param>
        /// <returns>True if both components have the same fluid content, false otherwise.</returns>
        public Boolean SameContent(FluidComponent other)
        {
            if (this.fluids.Count != other.fluids.Count) return false;

            foreach (KeyValuePair<ResourceType, float> entry in this.fluids)
            {
                if (!other.fluids.TryGetValue(entry.Key, out float otherValue))
                    return false;
                if (Math.Abs(otherValue - entry.Value) > 0.001f)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if this component contains, at least, the same
        /// fluid content as another fluid component, understanding "at least" as having 
        /// the same or more amount of each fluid type present in the other component, 
        /// regardless of the order of the fluids.
        /// </summary>
        /// <param name="other"> Another fluid component to compare with.</param>
        /// <returns>True if this component contains at least the same fluid content as the other, false otherwise.</returns>
        public Boolean ContainsAtLeast(FluidComponent other)
        {
            if (this.fluids.Count != other.fluids.Count) return false;

            foreach (KeyValuePair<ResourceType, float> entry in this.fluids)
            {
                if (!other.fluids.TryGetValue(entry.Key, out float otherValue))
                    return false;
                if (otherValue - entry.Value > 0.0f)
                    return false;
            }
            return true;
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is FluidComponent otherFluid &&
                this.maxCapacity == otherFluid.maxCapacity &&
                this.SameContent(otherFluid);

        }
    }
}
