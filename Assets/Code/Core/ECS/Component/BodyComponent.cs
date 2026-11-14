using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Base physical attributes of the character. Change very slowly or never.
    /// </summary>
    public class BodyComponent : BasicComponent
    {
        private float _height;
        private float _weight;
        private float _age;
        private int _sex; // 0 = male, 1 = female
        private float _fatPercentage;

        public BodyComponent(float height, float weight, float age, int sex)
        {
            _height = height;
            _weight = weight;
            _age = age;
            _sex = sex;
            EstimateFatPercentage();
            Name = "BodyComponent";
        }

        public float EstimateFatPercentage()
        {
            if (_sex == 0)
                _fatPercentage = 1.2f * Bmi() + 0.23f * _age - 16.2f;
            else
                _fatPercentage = 1.2f * Bmi() + 0.23f * _age - 5.4f;
            return _fatPercentage;
        }

        public float Bmi()
        {
            return _weight / (_height * _height);
        }

        /// <summary>
        /// Estimated muscle mass in kg.
        /// </summary>
        public float GetMuscleMass()
        {
            return _weight * (1f - _fatPercentage / 100f);
        }

        // Getters and Setters
        public float Height => _height;
        public void SetHeight(float height) => _height = height;

        public float Weight => _weight;
        public void SetWeight(float weight) => _weight = weight;

        public float Age => _age;
        public void SetAge(float age) => _age = age;

        public int Sex => _sex;
        public void SetSex(int sex) => _sex = sex;

        public float FatPercentage => _fatPercentage;
        public void SetFatPercentage(float fatPercentage) => _fatPercentage = fatPercentage;

        public override IComponent Clone()
        {
            var copy = new BodyComponent(_height, _weight, _age, _sex);
            copy._fatPercentage = _fatPercentage;
            copy.Name = Name;
            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is BodyComponent o)
            {
                float eps = 0.001f;
                return
                    Math.Abs(_height - o._height) < eps &&
                    Math.Abs(_weight - o._weight) < eps &&
                    Math.Abs(_age - o._age) < eps &&
                    _sex == o._sex &&
                    Math.Abs(_fatPercentage - o._fatPercentage) < eps;
            }
            return false;
        }
    }
}
