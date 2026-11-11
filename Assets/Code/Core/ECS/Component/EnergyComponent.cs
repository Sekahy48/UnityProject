using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Stamina, fatigue and metabolism. Changes every frame.
    /// </summary>
    public class EnergyComponent : BasicComponent
    {
        private float stamina;
        private float maxStamina;
        private float fatigue;
        private float maxFatigue;

        private float energeticBalance;
        private float metabolicRate;
        private float basalMetabolicRate;

        public EnergyComponent(float stamina, float maxStamina, float fatigue, float maxFatigue)
        {
            this.stamina = stamina;
            this.maxStamina = maxStamina;
            this.fatigue = fatigue;
            this.maxFatigue = maxFatigue;
            this._name = "EnergyComponent";
        }

        /// <summary>
        /// Calculates the basal metabolic rate (Mifflin-St Jeor) from body data.
        /// </summary>
        public float CalculateBasalMetabolism(float weight, float heightCm, float age, int sex)
        {
            if (sex == 0)
                basalMetabolicRate = 10 * weight + 6.25f * heightCm - 5 * age + 5;
            else
                basalMetabolicRate = 10 * weight + 6.25f * heightCm - 5 * age - 161;
            return basalMetabolicRate;
        }

        // Stamina
        public float Stamina => stamina;
        public void SetStamina(float stamina) => this.stamina = Math.Max(0, Math.Min(stamina, maxStamina));
        public float MaxStamina => maxStamina;
        public void SetMaxStamina(float maxStamina) => this.maxStamina = maxStamina;
        public bool IsStaminaFull() => stamina >= maxStamina;
        public bool IsStaminaEmpty() => stamina <= 0;

        // Fatigue
        public float Fatigue => fatigue;
        public void SetFatigue(float fatigue) => this.fatigue = Math.Max(0, Math.Min(fatigue, maxFatigue));
        public float MaxFatigue => maxFatigue;
        public void SetMaxFatigue(float maxFatigue) => this.maxFatigue = maxFatigue;
        public bool IsFatigueFull() => fatigue >= maxFatigue;
        public bool IsFatigueEmpty() => fatigue <= 0;

        // Metabolism
        public float EnergeticBalance => energeticBalance;
        public void SetEnergeticBalance(float energeticBalance) => this.energeticBalance = energeticBalance;
        public float MetabolicRate => metabolicRate;
        public void SetMetabolicRate(float metabolicRate) => this.metabolicRate = metabolicRate;
        public float BasalMetabolicRate => basalMetabolicRate;
        public void SetBasalMetabolicRate(float basalMetabolicRate) => this.basalMetabolicRate = basalMetabolicRate;

        public override IComponent Clone()
        {
            var copy = new EnergyComponent(stamina, maxStamina, fatigue, maxFatigue);
            copy.energeticBalance = this.energeticBalance;
            copy.metabolicRate = this.metabolicRate;
            copy.basalMetabolicRate = this.basalMetabolicRate;
            copy._name = this._name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is EnergyComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(stamina - o.stamina) < eps &&
                    Math.Abs(maxStamina - o.maxStamina) < eps &&
                    Math.Abs(fatigue - o.fatigue) < eps &&
                    Math.Abs(maxFatigue - o.maxFatigue) < eps &&
                    Math.Abs(energeticBalance - o.energeticBalance) < eps &&
                    Math.Abs(metabolicRate - o.metabolicRate) < eps &&
                    Math.Abs(basalMetabolicRate - o.basalMetabolicRate) < eps;
            }
            return false;
        }
    }
}
