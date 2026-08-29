using System;
using System.Collections.Generic;

namespace ECS.Component
{
    /// <summary>
    /// Component representing an entity's healing capacity.
    /// </summary>
    public class HealComponent : BasicComponent, IJsonLoadable
    {
        private int healingAmount;       // Base healing amount
        private float bonusMultiplier;   // Bonus multiplier

        public HealComponent() {}

        public HealComponent(int healingAmount, float bonusMultiplier)
        {
            this.healingAmount = healingAmount;
            this.bonusMultiplier = bonusMultiplier;
            this._name = "HealComponent"; // Initializes the component name
        }

        // Getters and setters in C# style
        public int HealingAmount
        {
            get => healingAmount;
            set => healingAmount = value;
        }

        public float BonusMultiplier
        {
            get => bonusMultiplier;
            set => bonusMultiplier = value;
        }

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("healingAmount")) HealingAmount = Convert.ToInt32(values["healingAmount"]);
            if (values.ContainsKey("bonusMultiplier")) BonusMultiplier = Convert.ToSingle(values["bonusMultiplier"]);
        }

        public int CalculateHealing()
        {
            return (int)(healingAmount * bonusMultiplier);
        }

        public override string ToString()
        {
            return $"HealComponent{{healingAmount={healingAmount}, bonusMultiplier={bonusMultiplier}}}";
        }

        public override IComponent Clone()
        {
            return new HealComponent(this.healingAmount, this.bonusMultiplier); // Clones the component
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is HealComponent otherHeal &&
                this.healingAmount == otherHeal.healingAmount &&
                this.bonusMultiplier == otherHeal.bonusMultiplier;
        }
    }
}
