using System;
using System.Collections.Generic;

namespace ECS.Component
{
    public class StorageComponent : IComponent, IJsonLoadable
    {
        private float maxVolume;
        private float maxWeight;
        private float weightRatio;

        public StorageComponent() {}

        public StorageComponent(float maxVolume, float maxWeight, float weightRatio)
        {
            this.maxVolume = maxVolume;
            this.maxWeight = maxWeight;
            this.weightRatio = weightRatio;
        }

        public float MaxVolume => maxVolume;
        public void SetMaxVolume(float value) { maxVolume = value; }

        public float MaxWeight => maxWeight;
        public void SetMaxWeight(float value) { maxWeight = value; }

        public float WeightRatio => weightRatio;
        public void SetWeightRatio(float value) { weightRatio = value; }

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("maxVolume")) SetMaxVolume(Convert.ToSingle(values["maxVolume"]));
            if (values.ContainsKey("maxWeight")) SetMaxWeight(Convert.ToSingle(values["maxWeight"]));
            if (values.ContainsKey("weightRatio")) SetWeightRatio(Convert.ToSingle(values["weightRatio"]));
        }

        public IComponent Clone()
        {
            return new StorageComponent(this.maxVolume, this.maxWeight, this.weightRatio);
        }

        public bool Equivalent(IComponent other)
        {
            return 
                other is StorageComponent otherStorage &&
                this.maxVolume == otherStorage.maxVolume &&
                this.maxWeight == otherStorage.maxWeight &&
                this.weightRatio == otherStorage.weightRatio;
        }
    }
}