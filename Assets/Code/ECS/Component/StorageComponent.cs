namespace ECS.Component
{
    public class StorageComponent : IComponent
    {
        private float maxVolume;
        private float maxWeight;
        private float weightRatio;

        public StorageComponent(float maxVolume, float maxWeight, float weightRatio)
        {
            this.maxVolume = maxVolume;
            this.maxWeight = maxWeight;
            this.weightRatio = weightRatio;
        }

        public float getMaxVolume()
        {
            return maxVolume;
        }

        public float getMaxWeight()
        {
            return maxWeight;
        }

        public float getWeightRatio()
        {
            return weightRatio;
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