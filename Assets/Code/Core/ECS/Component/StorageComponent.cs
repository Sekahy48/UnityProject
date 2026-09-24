using System.Collections.Generic;

namespace Core.ECS.Component
{
    public class StorageComponent : IComponent, IJsonLoadable, INumericFields
    {
        private int _gridH, _gridW;
        private float _maxWeight; 

        public StorageComponent() {}

        public StorageComponent(int gridH, int gridW, float maxWeight)
        {
            _gridH = gridH;
            _gridW = gridW;
            _maxWeight = maxWeight;
        }  

        public int GridH => _gridH;
        public int GridW => _gridW;
        public float MaxWeight => _maxWeight;
        public void SetMaxWeight(float value) { _maxWeight = value; }  

        private static readonly NumericFields<StorageComponent> Fields =
            new NumericFields<StorageComponent>()
                .Add("gridH", c => c._gridH, (c, v) => c._gridH = (int)v)
                .Add("gridW", c => c._gridW, (c, v) => c._gridW = (int)v)
                .Add("maxWeight", c => c._maxWeight, (c, v) => c.SetMaxWeight(v));

        public bool TryGetNumericValue(string field, out float value)
            => Fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            Fields.Apply(this, values);
        }

        public IComponent Clone()
        {
            return new StorageComponent(_gridH, _gridW, _maxWeight);
        }

        public bool Equivalent(IComponent other)
        {
            return 
                other is StorageComponent otherStorage &&
                _gridH == otherStorage._gridH &&
                _gridW == otherStorage._gridW &&
                _maxWeight == otherStorage._maxWeight;
        }
    }
}