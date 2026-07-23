using System;
using System.Collections.Generic;

namespace ECS.Component
{
    public class StorageComponent : IComponent, IJsonLoadable
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

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("gridH")) _gridH = Convert.ToInt32(values["gridH"]);
            if (values.ContainsKey("gridW")) _gridW = Convert.ToInt32(values["gridW"]);
            if (values.ContainsKey("maxWeight")) SetMaxWeight(Convert.ToSingle(values["maxWeight"]));
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