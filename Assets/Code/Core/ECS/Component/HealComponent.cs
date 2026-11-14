using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    /// <summary>
    /// Component representing an entity's healing capacity.
    /// </summary>
    public class HealComponent : BasicComponent, IJsonLoadable, INumericFields
    {
        private int _healingAmount;       // Base healing amount
        private float _bonusMultiplier;   // Bonus multiplier

        public HealComponent() {}

        public HealComponent(int healingAmount, float bonusMultiplier)
        {
            _healingAmount = healingAmount;
            _bonusMultiplier = bonusMultiplier;
            Name = "HealComponent"; // Initializes the component name
        }

        // Getters and setters in C# style
        public int HealingAmount
        {
            get => _healingAmount;
            set => _healingAmount = value;
        }

        public float BonusMultiplier
        {
            get => _bonusMultiplier;
            set => _bonusMultiplier = value;
        }

        private static readonly NumericFields<HealComponent> _fields =
            new NumericFields<HealComponent>()
                .Add("healingAmount", c => c._healingAmount, (c, v) => c.HealingAmount = (int)v)
                .Add("bonusMultiplier", c => c._bonusMultiplier, (c, v) => c.BonusMultiplier = v);

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);
        }

        public int CalculateHealing()
        {
            return (int)(_healingAmount * _bonusMultiplier);
        }

        public override string ToString()
        {
            return $"HealComponent{{_healingAmount={_healingAmount}, _bonusMultiplier={_bonusMultiplier}}}";
        }

        public override IComponent Clone()
        {
            return new HealComponent(_healingAmount, _bonusMultiplier); // Clones the component
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is HealComponent otherHeal &&
                _healingAmount == otherHeal._healingAmount &&
                _bonusMultiplier == otherHeal._bonusMultiplier;
        }
    }
}
