using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    /// <summary>
    /// Hunger, thirst, macronutrients and reserves. Relevant in Phase 3.
    /// </summary>
    public class NutritionComponent : BasicComponent, IJsonLoadable, INumericFields
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

        public NutritionComponent() {}

        public NutritionComponent(float maxHunger, float maxThirst)
        {
            this.maxHunger = maxHunger;
            this.maxThirst = maxThirst;
            this.hunger = RndmF(0, maxHunger);
            this.thirst = RndmF(0, maxThirst);
            this._name = "NutritionComponent";
        }

        /// <summary>
        /// Generates the stored body water from weight and sex.
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
        public float Hunger => hunger;
        public void SetHunger(float hunger) => this.hunger = Math.Max(0, Math.Min(hunger, maxHunger));
        public float MaxHunger => maxHunger;
        public void SetMaxHunger(float maxHunger) => this.maxHunger = maxHunger;
        public bool IsHungerFull() => hunger >= maxHunger;
        public bool IsHungerEmpty() => hunger <= 0;

        // Thirst
        public float Thirst => thirst;
        public void SetThirst(float thirst) => this.thirst = Math.Max(0, Math.Min(thirst, maxThirst));
        public float MaxThirst => maxThirst;
        public void SetMaxThirst(float maxThirst) => this.maxThirst = maxThirst;
        public bool IsThirstFull() => thirst >= maxThirst;
        public bool IsThirstEmpty() => thirst <= 0;

        // Reserves
        public float StoredKcal => storedKcal;
        public void SetStoredKcal(float storedKcal) => this.storedKcal = storedKcal;
        public float StoredWater => storedWater;
        public void SetStoredWater(float storedWater) => this.storedWater = storedWater;

        // Macronutrients
        public float Protein => protein;
        public void AddProtein(float protein) => this.protein += protein;
        public void SetProtein(float protein) => this.protein = protein;

        public float Carbohydrates => carbohydrates;
        public void AddCarbohydrates(float carbohydrates) => this.carbohydrates += carbohydrates;
        public void SetCarbohydrates(float carbohydrates) => this.carbohydrates = carbohydrates;

        public float Fats => fats;
        public void AddFats(float fats) => this.fats += fats;
        public void SetFats(float fats) => this.fats = fats;

        public float Micronutrients => micronutrients;
        public void AddMicronutrients(float micronutrients) => this.micronutrients += micronutrients;
        public void SetMicronutrients(float micronutrients) => this.micronutrients = micronutrients;

        public float Fiber => fiber;
        public void AddFiber(float fiber) => this.fiber += fiber;
        public void SetFiber(float fiber) => this.fiber = fiber;

        /// <summary>
        /// Los maximos van declarados antes que los valores que recortan, y el orden de esta
        /// lista es el orden en que se aplican: <c>SetHunger</c> limita contra
        /// <c>maxHunger</c>, asi que al reves el hambre se recortaria contra cero.
        /// </summary>
        private static readonly NumericFields<NutritionComponent> Fields =
            new NumericFields<NutritionComponent>()
                .Add("maxHunger", c => c.maxHunger, (c, v) => c.SetMaxHunger(v))
                .Add("maxThirst", c => c.maxThirst, (c, v) => c.SetMaxThirst(v))
                .Add("hunger", c => c.hunger, (c, v) => c.SetHunger(v))
                .Add("thirst", c => c.thirst, (c, v) => c.SetThirst(v))
                .Add("storedKcal", c => c.storedKcal, (c, v) => c.SetStoredKcal(v))
                .Add("storedWater", c => c.storedWater, (c, v) => c.SetStoredWater(v))
                .Add("protein", c => c.protein, (c, v) => c.SetProtein(v))
                .Add("carbohydrates", c => c.carbohydrates, (c, v) => c.SetCarbohydrates(v))
                .Add("fats", c => c.fats, (c, v) => c.SetFats(v))
                .Add("micronutrients", c => c.micronutrients, (c, v) => c.SetMicronutrients(v))
                .Add("fiber", c => c.fiber, (c, v) => c.SetFiber(v));

        public bool TryGetNumericValue(string field, out float value)
            => Fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            Fields.Apply(this, values);
        }

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
