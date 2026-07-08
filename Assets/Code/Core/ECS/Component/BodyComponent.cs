using System;

namespace ECS.Component
{
    /// <summary>
    /// Atributos físicos base del personaje. Cambian muy lentamente o nunca.
    /// </summary>
    public class BodyComponent : BasicComponent
    {
        private float height;
        private float weight;
        private float age;
        private int sex; // 0 = hombre, 1 = mujer
        private float fatPercentage;

        public BodyComponent(float height, float weight, float age, int sex)
        {
            this.height = height;
            this.weight = weight;
            this.age = age;
            this.sex = sex;
            EstimateFatPercentage();
            this._name = "BodyComponent";
        }

        public float EstimateFatPercentage()
        {
            if (sex == 0)
                fatPercentage = 1.2f * Bmi() + 0.23f * age - 16.2f;
            else
                fatPercentage = 1.2f * Bmi() + 0.23f * age - 5.4f;
            return fatPercentage;
        }

        public float Bmi()
        {
            return weight / (height * height);
        }

        /// <summary>
        /// Masa muscular estimada en kg.
        /// </summary>
        public float GetMuscleMass()
        {
            return weight * (1f - fatPercentage / 100f);
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

        public override IComponent Clone()
        {
            var copy = new BodyComponent(height, weight, age, sex);
            copy.fatPercentage = this.fatPercentage;
            copy._name = this._name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is BodyComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(height - o.height) < eps &&
                    Math.Abs(weight - o.weight) < eps &&
                    Math.Abs(age - o.age) < eps &&
                    sex == o.sex &&
                    Math.Abs(fatPercentage - o.fatPercentage) < eps;
            }
            return false;
        }
    }
}
