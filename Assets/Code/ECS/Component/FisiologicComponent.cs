using System;

namespace ECS.Component
{
    public class FisiologicComponent : BasicComponent
    {
        private static Random random = new Random(); // Generador de números aleatorios

        private float height; // Altura del personaje
        private float weight; // Peso del personaje
        private float age;    // Edad del personaje
        private int sex;      // Sexo del personaje (0 = hombre, 1 = mujer)
        private float fatPercentage; // Porcentaje de grasa corporal

        private float energeticBalance;   // Balance energético
        private float metabolicRate;      // Tasa metabólica
        private float basalMetabolicRate; // Tasa metabólica basal

        private float hunger;   // Hambre
        private float thirst;   // Sed
        private float fatigue;  // Fatiga

        private float maxHunger;
        private float maxThirst;
        private float maxFatigue;

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

        public FisiologicComponent(float height, float weight, float age, int sex)
        {
            this.sex = sex;
            this.height = height;
            this.weight = weight;
            this.age = age;
            this.energeticBalance = RndmF(-2000, 2000);
            this.metabolicRate = RndmF(100, 150);
            CalculateBasalMetabolism();
            this.hunger = RndmF(0, 100);
            this.thirst = RndmF(0, 100);
            this.fatigue = RndmF(0, 100);
            this.maxHunger = 100;
            this.maxThirst = 100;
            this.maxFatigue = 100;
            EstimateFatPercentage();
            this.storedWater = GenerateStoredWater();

            this.name = "FisiologicComponent";
        }

        public float EstimateFatPercentage()
        {
            if (this.sex == 0)
            {
                this.fatPercentage = 1.2f * Bmi() + 0.23f * this.age - 16.2f;
            }
            else
            {
                this.fatPercentage = 1.2f * Bmi() + 0.23f * this.age - 5.4f;
            }
            return this.fatPercentage;
        }

        public float Bmi()
        {
            return this.weight / (this.height * this.height);
        }

        public float GenerateStoredWater()
        {
            float water;
            if (this.sex == 0)
            {
                water = (weight * RndmF(55, 65)) / 100;
            }
            else
            {
                water = (weight * RndmF(45, 55)) / 100;
            }
            return water;
        }

        public float CalculateBasalMetabolism()
        {
            if (sex == 0)
            {
                this.basalMetabolicRate = 10 * weight + 6.25f * (height * 100) - 5 * age + 5;
            }
            else
            {
                this.basalMetabolicRate = 10 * weight + 6.25f * (height * 100) - 5 * age - 161;
            }
            return this.basalMetabolicRate;
        }

        // Getters y Setters
        public float GetHeight() => height;
        public void SetHeight(float height) => this.height = height;

        public float GetWeight() => weight;
        public void SetWeight(float weight) => this.weight = weight;

        public float GetAge() => age;
        public void SetAge(float age) => this.age = age;

        public int GetSex() => sex;
        public void SetSex(int sex) => this.sex = sex;

        public float GetFatPercentage() => fatPercentage;
        public void SetFatPercentage(float fatPercentage) => this.fatPercentage = fatPercentage;

        public float GetEnergeticBalance() => energeticBalance;
        public void SetEnergeticBalance(float energeticBalance) => this.energeticBalance = energeticBalance;

        public float GetMetabolicRate() => metabolicRate;
        public void SetMetabolicRate(float metabolicRate) => this.metabolicRate = metabolicRate;

        public float GetBasalMetabolicRate() => basalMetabolicRate;
        public void SetBasalMetabolicRate(float basalMetabolicRate) => this.basalMetabolicRate = basalMetabolicRate;

        public float GetHunger() => hunger;
        public void SetHunger(float hunger) => this.hunger = hunger;

        public float GetThirst() => thirst;
        public void SetThirst(float thirst) => this.thirst = thirst;

        public float GetFatigue() => fatigue;
        public void SetFatigue(float fatigue) => this.fatigue = fatigue;

        public float GetMaxHunger() => maxHunger;
        public void SetMaxHunger(float maxHunger) => this.maxHunger = maxHunger;

        public float GetMaxThirst() => maxThirst;
        public void SetMaxThirst(float maxThirst) => this.maxThirst = maxThirst;

        public float GetMaxFatigue() => maxFatigue;
        public void SetMaxFatigue(float maxFatigue) => this.maxFatigue = maxFatigue;

        public float GetStoredKcal() => storedKcal;
        public void SetStoredKcal(float storedKcal) => this.storedKcal = storedKcal;

        public float GetStoredWater() => storedWater;
        public void SetStoredWater(float storedWater) => this.storedWater = storedWater;

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

        public void SetThirst(float thirst, float maxThirst) => this.thirst = Math.Min(thirst, maxThirst);
        public void SetHunger(float hunger, float maxHunger) => this.hunger = Math.Min(hunger, maxHunger);
        public void SetFatigue(float fatigue, float maxFatigue) => this.fatigue = Math.Min(fatigue, maxFatigue);

        public override IComponent Clone()
        {
            FisiologicComponent copy = new FisiologicComponent(this.height, this.weight, this.age, this.sex);

            copy.fatPercentage = this.fatPercentage;
            copy.energeticBalance = this.energeticBalance;
            copy.metabolicRate = this.metabolicRate;
            copy.basalMetabolicRate = this.basalMetabolicRate;

            copy.hunger = this.hunger;
            copy.thirst = this.thirst;
            copy.fatigue = this.fatigue;

            copy.maxHunger = this.maxHunger;
            copy.maxThirst = this.maxThirst;
            copy.maxFatigue = this.maxFatigue;

            copy.storedKcal = this.storedKcal;
            copy.storedWater = this.storedWater;

            copy.protein = this.protein;
            copy.carbohydrates = this.carbohydrates;
            copy.fats = this.fats;
            copy.micronutrients = this.micronutrients;
            copy.fiber = this.fiber;

            copy.name = this.name;

            return copy;
        }
    }
}
