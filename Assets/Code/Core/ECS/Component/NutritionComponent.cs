using System;

namespace ECS.Component
{
    /// <summary>
    /// Hambre, sed, macronutrientes y reservas. Relevante en Fase 3.
    /// </summary>
    public class NutritionComponent : BasicComponent
    {
        private static readonly Random random = new Random();

        private float hunger;
        private float thirst;
        private float maxHunger;
        private float maxThirst;

        private float storedKcal;
        private float storedWater;

        private float protein;
        private float carbohydrates;
        private float fats;
        private float micronutrients;
        private float fiber;

        private float RndmF(float min, float max)
        {
            return (float)(min + (max - min) * random.NextDouble());
        }

        public NutritionComponent(float maxHunger, float maxThirst)
        {
            this.maxHunger = maxHunger;
            this.maxThirst = maxThirst;
            this.hunger = RndmF(0, maxHunger);
            this.thirst = RndmF(0, maxThirst);
            this._name = "NutritionComponent";
        }

        /// <summary>
        /// Genera el agua corporal almacenada a partir del peso y sexo.
        /// </summary>
        public float GenerateStoredWater(float weight, int sex)
        {
            if (sex == 0)
                storedWater = (weight * RndmF(55, 65)) / 100;
            else
                storedWater = (weight * RndmF(45, 55)) / 100;
            return storedWater;
        }

        // Hunger
        public float GetHunger() => hunger;
        public void SetHunger(float hunger) => this.hunger = Math.Max(0, Math.Min(hunger, maxHunger));
        public float GetMaxHunger() => maxHunger;
        public void SetMaxHunger(float maxHunger) => this.maxHunger = maxHunger;
        public bool IsHungerFull() => hunger >= maxHunger;
        public bool IsHungerEmpty() => hunger <= 0;

        // Thirst
        public float GetThirst() => thirst;
        public void SetThirst(float thirst) => this.thirst = Math.Max(0, Math.Min(thirst, maxThirst));
        public float GetMaxThirst() => maxThirst;
        public void SetMaxThirst(float maxThirst) => this.maxThirst = maxThirst;
        public bool IsThirstFull() => thirst >= maxThirst;
        public bool IsThirstEmpty() => thirst <= 0;

        // Reserves
        public float GetStoredKcal() => storedKcal;
        public void SetStoredKcal(float storedKcal) => this.storedKcal = storedKcal;
        public float GetStoredWater() => storedWater;
        public void SetStoredWater(float storedWater) => this.storedWater = storedWater;

        // Macronutrients
        public float GetProtein() => protein;
        public void AddProtein(float protein) => this.protein += protein;
        public void SetProtein(float protein) => this.protein = protein;

        public float GetCarbohydrates() => carbohydrates;
        public void AddCarbohydrates(float carbohydrates) => this.carbohydrates += carbohydrates;
        public void SetCarbohydrates(float carbohydrates) => this.carbohydrates = carbohydrates;

        public float GetFats() => fats;
        public void AddFats(float fats) => this.fats += fats;
        public void SetFats(float fats) => this.fats = fats;

        public float GetMicronutrients() => micronutrients;
        public void AddMicronutrients(float micronutrients) => this.micronutrients += micronutrients;
        public void SetMicronutrients(float micronutrients) => this.micronutrients = micronutrients;

        public float GetFiber() => fiber;
        public void AddFiber(float fiber) => this.fiber += fiber;
        public void SetFiber(float fiber) => this.fiber = fiber;

        public override IComponent Clone()
        {
            var copy = new NutritionComponent(maxHunger, maxThirst);
            copy.hunger = this.hunger;
            copy.thirst = this.thirst;
            copy.storedKcal = this.storedKcal;
            copy.storedWater = this.storedWater;
            copy.protein = this.protein;
            copy.carbohydrates = this.carbohydrates;
            copy.fats = this.fats;
            copy.micronutrients = this.micronutrients;
            copy.fiber = this.fiber;
            copy._name = this._name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is NutritionComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(hunger - o.hunger) < eps &&
                    Math.Abs(thirst - o.thirst) < eps &&
                    Math.Abs(maxHunger - o.maxHunger) < eps &&
                    Math.Abs(maxThirst - o.maxThirst) < eps &&
                    Math.Abs(storedKcal - o.storedKcal) < eps &&
                    Math.Abs(storedWater - o.storedWater) < eps &&
                    Math.Abs(protein - o.protein) < eps &&
                    Math.Abs(carbohydrates - o.carbohydrates) < eps &&
                    Math.Abs(fats - o.fats) < eps &&
                    Math.Abs(micronutrients - o.micronutrients) < eps &&
                    Math.Abs(fiber - o.fiber) < eps;
            }
            return false;
        }
    }
}
