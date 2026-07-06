using System; 

namespace ECS.Component
{
    public class FisiologicComponent : BasicComponent
    {
        private static System.Random random = new System.Random(); // Generador de números aleatorios

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
        private float stamina; // Resistencia física
        private float maxHunger;
        private float maxThirst;
        private float maxFatigue;
        private float maxStamina;

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
            this.maxHunger = 100;
            this.maxThirst = 100;
            this.fatigue = 100f;
            this.maxFatigue = 100f;
            EstimateFatPercentage();
            this.storedWater = GenerateStoredWater();
            this._name = "FisiologicComponent";
            this.stamina = 100f;
            this.maxStamina = 100f;
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
        public void SetHunger(float hunger) => this.hunger = Math.Max(0, Math.Min(hunger, this.maxHunger));

        public float GetThirst() => thirst;
        public void SetThirst(float thirst) => this.thirst = Math.Max(0, Math.Min(thirst, this.maxThirst));

        public float GetFatigue() => fatigue;
        public void SetFatigue(float fatigue) => this.fatigue = Math.Max(0, Math.Min(fatigue, this.maxFatigue));

        public float GetStamina() => stamina;
        public void SetStamina(float stamina) => this.stamina = Math.Max(0, Math.Min(stamina, this.maxStamina));
        
        public float GetMaxStamina() => maxStamina;
        public void SetMaxStamina(float maxStamina) => this.maxStamina = maxStamina;

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


        // Metodos para comprobar si un atributo esta al maximo posible
        public bool IsHungerFull() => this.hunger >= this.maxHunger;
        public bool IsThirstFull() => this.thirst >= this.maxThirst;
        public bool IsFatigueFull() => this.fatigue >= this.maxFatigue;
        public bool IsStaminaFull() => this.stamina >= this.maxStamina;
        public bool IsHungerEmpty() => this.hunger <= 0;
        public bool IsThirstEmpty() => this.thirst <= 0;
        public bool IsFatigueEmpty() => this.fatigue <= 0;
        public bool IsStaminaEmpty() => this.stamina <= 0;

        // FisiologicComponent
        public float GetMaxCarryWeight()
        {
            float muscleMass = weight * (1f - fatPercentage / 100f);
            float carryBase = muscleMass * 0.5f;
            float factorSex = (sex == 0) ? 1.0f : 0.85f;
            
            float factorAge;
            if      (age < 18f)  factorAge = 0.6f  + (age - 10f) * 0.05f;
            else if (age <= 35f) factorAge = 1.0f;
            else if (age <= 60f) factorAge = 1.0f  - (age - 35f) * 0.015f;
            else                 factorAge = 0.625f - (age - 60f) * 0.01f;
            factorAge = Math.Max(factorAge, 0.1f);

            float hungerNorm  = hunger  / maxHunger;
            float thirstNorm  = thirst  / maxThirst;
            float fatigueNorm = fatigue / maxFatigue;

            float factorHunger  = 1.0f - hungerNorm  * 0.30f;
            float factorThirst  = 1.0f - thirstNorm  * 0.40f;
            float factorFatigue = 1.0f - fatigueNorm * 0.35f;

            return carryBase * factorSex * factorAge * factorHunger * factorThirst * factorFatigue;
        }

        public float GetMaxCarryVolume()
        {
            float muscleMass = weight * (1f - fatPercentage / 100f);
            float heightCm   = height * 100f;
            float volumeBase = 8.0f + (heightCm - 170f) * 0.05f + muscleMass * 0.1f;

            float factorHunger  = 1.0f - (hunger  / maxHunger)  * 0.30f;
            float factorThirst  = 1.0f - (thirst  / maxThirst)  * 0.40f;
            float factorFatigue = 1.0f - (fatigue / maxFatigue) * 0.35f;

            return volumeBase * factorHunger * factorThirst * factorFatigue;
        }

        // Clonación del componente        
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
            copy.stamina = this.stamina;

            copy.maxStamina = this.maxStamina;
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

            copy._name = this._name;

            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            
            if (other is FisiologicComponent otherFisio)
            {
                float eps = 0.001f;
                return
                    Math.Abs(height         - otherFisio.height)         < eps &&
                    Math.Abs(weight         - otherFisio.weight)         < eps &&
                    Math.Abs(age            - otherFisio.age)            < eps &&
                    sex == otherFisio.sex                                      &&
                    Math.Abs(fatPercentage  - otherFisio.fatPercentage)  < eps &&
                    Math.Abs(hunger         - otherFisio.hunger)         < eps &&
                    Math.Abs(thirst         - otherFisio.thirst)         < eps &&
                    Math.Abs(fatigue        - otherFisio.fatigue)        < eps &&
                    Math.Abs(stamina        - otherFisio.stamina)        < eps &&
                    Math.Abs(maxHunger      - otherFisio.maxHunger)      < eps &&
                    Math.Abs(maxThirst      - otherFisio.maxThirst)      < eps &&
                    Math.Abs(maxFatigue     - otherFisio.maxFatigue)     < eps &&
                    Math.Abs(maxStamina     - otherFisio.maxStamina)     < eps &&
                    Math.Abs(storedKcal     - otherFisio.storedKcal)     < eps &&
                    Math.Abs(storedWater    - otherFisio.storedWater)    < eps &&
                    Math.Abs(protein        - otherFisio.protein)        < eps &&
                    Math.Abs(carbohydrates  - otherFisio.carbohydrates)  < eps &&
                    Math.Abs(fats           - otherFisio.fats)           < eps &&
                    Math.Abs(micronutrients - otherFisio.micronutrients) < eps &&
                    Math.Abs(fiber          - otherFisio.fiber)          < eps &&
                    Math.Abs(energeticBalance   - otherFisio.energeticBalance)   < eps &&
                    Math.Abs(metabolicRate      - otherFisio.metabolicRate)      < eps &&
                    Math.Abs(basalMetabolicRate - otherFisio.basalMetabolicRate) < eps;
            }
            return false;
        }
    }
}
