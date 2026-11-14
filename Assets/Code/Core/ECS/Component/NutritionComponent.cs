using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    /// <summary>
    /// Hunger, thirst, macronutrients and reserves. Relevant in Phase 3.
    /// </summary>
    public class NutritionComponent : BasicComponent, IJsonLoadable, INumericFields
    {
        private static readonly Random _random = new Random();

        private float _hunger;
        private float _thirst;
        private float _maxHunger;
        private float _maxThirst;

        private float _storedKcal;
        private float _storedWater;

        private float _protein;
        private float _carbohydrates;
        private float _fats;
        private float _micronutrients;
        private float _fiber;

        private float RndmF(float min, float max)
        {
            return (float)(min + (max - min) * _random.NextDouble());
        }

        public NutritionComponent() {}

        public NutritionComponent(float maxHunger, float maxThirst)
        {
            _maxHunger = maxHunger;
            _maxThirst = maxThirst;
            _hunger = RndmF(0, maxHunger);
            _thirst = RndmF(0, maxThirst);
            Name = "NutritionComponent";
        }

        /// <summary>
        /// Generates the stored body water from weight and sex.
        /// </summary>
        public float GenerateStoredWater(float weight, int sex)
        {
            if (sex == 0)
                _storedWater = (weight * RndmF(55, 65)) / 100;
            else
                _storedWater = (weight * RndmF(45, 55)) / 100;
            return _storedWater;
        }

        // Hunger
        public float Hunger => _hunger;
        public void SetHunger(float hunger) => _hunger = Math.Max(0, Math.Min(hunger, _maxHunger));
        public float MaxHunger => _maxHunger;
        public void SetMaxHunger(float maxHunger) => _maxHunger = maxHunger;
        public bool IsHungerFull() => _hunger >= _maxHunger;
        public bool IsHungerEmpty() => _hunger <= 0;

        // Thirst
        public float Thirst => _thirst;
        public void SetThirst(float thirst) => _thirst = Math.Max(0, Math.Min(thirst, _maxThirst));
        public float MaxThirst => _maxThirst;
        public void SetMaxThirst(float maxThirst) => _maxThirst = maxThirst;
        public bool IsThirstFull() => _thirst >= _maxThirst;
        public bool IsThirstEmpty() => _thirst <= 0;

        // Reserves
        public float StoredKcal => _storedKcal;
        public void SetStoredKcal(float storedKcal) => _storedKcal = storedKcal;
        public float StoredWater => _storedWater;
        public void SetStoredWater(float storedWater) => _storedWater = storedWater;

        // Macronutrients
        public float Protein => _protein;
        public void AddProtein(float protein) => _protein += protein;
        public void SetProtein(float protein) => _protein = protein;

        public float Carbohydrates => _carbohydrates;
        public void AddCarbohydrates(float carbohydrates) => _carbohydrates += carbohydrates;
        public void SetCarbohydrates(float carbohydrates) => _carbohydrates = carbohydrates;

        public float Fats => _fats;
        public void AddFats(float fats) => _fats += fats;
        public void SetFats(float fats) => _fats = fats;

        public float Micronutrients => _micronutrients;
        public void AddMicronutrients(float micronutrients) => _micronutrients += micronutrients;
        public void SetMicronutrients(float micronutrients) => _micronutrients = micronutrients;

        public float Fiber => _fiber;
        public void AddFiber(float fiber) => _fiber += fiber;
        public void SetFiber(float fiber) => _fiber = fiber;

        /// <summary>
        /// Los maximos van declarados antes que los valores que recortan, y el orden de esta
        /// lista es el orden en que se aplican: <c>SetHunger</c> limita contra
        /// <c>maxHunger</c>, asi que al reves el hambre se recortaria contra cero.
        /// </summary>
        private static readonly NumericFields<NutritionComponent> _fields =
            new NumericFields<NutritionComponent>()
                .Add("maxHunger", c => c._maxHunger, (c, v) => c.SetMaxHunger(v))
                .Add("maxThirst", c => c._maxThirst, (c, v) => c.SetMaxThirst(v))
                .Add("hunger", c => c._hunger, (c, v) => c.SetHunger(v))
                .Add("thirst", c => c._thirst, (c, v) => c.SetThirst(v))
                .Add("storedKcal", c => c._storedKcal, (c, v) => c.SetStoredKcal(v))
                .Add("storedWater", c => c._storedWater, (c, v) => c.SetStoredWater(v))
                .Add("protein", c => c._protein, (c, v) => c.SetProtein(v))
                .Add("carbohydrates", c => c._carbohydrates, (c, v) => c.SetCarbohydrates(v))
                .Add("fats", c => c._fats, (c, v) => c.SetFats(v))
                .Add("micronutrients", c => c._micronutrients, (c, v) => c.SetMicronutrients(v))
                .Add("fiber", c => c._fiber, (c, v) => c.SetFiber(v));

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);
        }

        public override IComponent Clone()
        {
            var copy = new NutritionComponent(_maxHunger, _maxThirst);
            copy._hunger = _hunger;
            copy._thirst = _thirst;
            copy._storedKcal = _storedKcal;
            copy._storedWater = _storedWater;
            copy._protein = _protein;
            copy._carbohydrates = _carbohydrates;
            copy._fats = _fats;
            copy._micronutrients = _micronutrients;
            copy._fiber = _fiber;
            copy.Name = Name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is NutritionComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(_hunger - o._hunger) < eps &&
                    Math.Abs(_thirst - o._thirst) < eps &&
                    Math.Abs(_maxHunger - o._maxHunger) < eps &&
                    Math.Abs(_maxThirst - o._maxThirst) < eps &&
                    Math.Abs(_storedKcal - o._storedKcal) < eps &&
                    Math.Abs(_storedWater - o._storedWater) < eps &&
                    Math.Abs(_protein - o._protein) < eps &&
                    Math.Abs(_carbohydrates - o._carbohydrates) < eps &&
                    Math.Abs(_fats - o._fats) < eps &&
                    Math.Abs(_micronutrients - o._micronutrients) < eps &&
                    Math.Abs(_fiber - o._fiber) < eps;
            }
            return false;
        }
    }
}
