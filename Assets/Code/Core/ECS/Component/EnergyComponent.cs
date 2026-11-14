using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Stamina, fatigue and metabolism. Changes every frame.
    /// </summary>
    public class EnergyComponent : BasicComponent
    {
        private float _stamina;
        private float _maxStamina;
        private float _fatigue;
        private float _maxFatigue;

        private float _energeticBalance;
        private float _metabolicRate;
        private float _basalMetabolicRate;

        public EnergyComponent(float stamina, float maxStamina, float fatigue, float maxFatigue)
        {
            _stamina = stamina;
            _maxStamina = maxStamina;
            _fatigue = fatigue;
            _maxFatigue = maxFatigue;
            Name = "EnergyComponent";
        }

        /// <summary>
        /// Calculates the basal metabolic rate (Mifflin-St Jeor) from body data.
        /// </summary>
        public float CalculateBasalMetabolism(float weight, float heightCm, float age, int sex)
        {
            if (sex == 0)
                _basalMetabolicRate = 10 * weight + 6.25f * heightCm - 5 * age + 5;
            else
                _basalMetabolicRate = 10 * weight + 6.25f * heightCm - 5 * age - 161;
            return _basalMetabolicRate;
        }

        // Stamina
        public float Stamina => _stamina;
        public void SetStamina(float stamina) => _stamina = Math.Max(0, Math.Min(stamina, _maxStamina));
        public float MaxStamina => _maxStamina;
        public void SetMaxStamina(float maxStamina) => _maxStamina = maxStamina;
        public bool IsStaminaFull() => _stamina >= _maxStamina;
        public bool IsStaminaEmpty() => _stamina <= 0;

        // Fatigue
        public float Fatigue => _fatigue;
        public void SetFatigue(float fatigue) => _fatigue = Math.Max(0, Math.Min(fatigue, _maxFatigue));
        public float MaxFatigue => _maxFatigue;
        public void SetMaxFatigue(float maxFatigue) => _maxFatigue = maxFatigue;
        public bool IsFatigueFull() => _fatigue >= _maxFatigue;
        public bool IsFatigueEmpty() => _fatigue <= 0;

        // Metabolism
        public float EnergeticBalance => _energeticBalance;
        public void SetEnergeticBalance(float energeticBalance) => _energeticBalance = energeticBalance;
        public float MetabolicRate => _metabolicRate;
        public void SetMetabolicRate(float metabolicRate) => _metabolicRate = metabolicRate;
        public float BasalMetabolicRate => _basalMetabolicRate;
        public void SetBasalMetabolicRate(float basalMetabolicRate) => _basalMetabolicRate = basalMetabolicRate;

        public override IComponent Clone()
        {
            var copy = new EnergyComponent(_stamina, _maxStamina, _fatigue, _maxFatigue);
            copy._energeticBalance = _energeticBalance;
            copy._metabolicRate = _metabolicRate;
            copy._basalMetabolicRate = _basalMetabolicRate;
            copy.Name = Name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is EnergyComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(_stamina - o._stamina) < eps &&
                    Math.Abs(_maxStamina - o._maxStamina) < eps &&
                    Math.Abs(_fatigue - o._fatigue) < eps &&
                    Math.Abs(_maxFatigue - o._maxFatigue) < eps &&
                    Math.Abs(_energeticBalance - o._energeticBalance) < eps &&
                    Math.Abs(_metabolicRate - o._metabolicRate) < eps &&
                    Math.Abs(_basalMetabolicRate - o._basalMetabolicRate) < eps;
            }
            return false;
        }
    }
}
