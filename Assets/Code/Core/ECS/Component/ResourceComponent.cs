using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    public class ResourceComponent : BasicComponent, IJsonLoadable, INumericFields
    {
        private ResourceType _type;
        private int _amount;
        private int _maxAmount; // Cantidad maxima de recurso
        private bool _renewable;

        public ResourceComponent() {}

        public ResourceComponent(ResourceType type, int amount, bool renewable)
        {
            _type = type;
            _amount = amount;
            _maxAmount = amount;
            _renewable = renewable;
            Name = "ResourceComponent"; // Nombre del componente
        }

        public ResourceType ResourceType => _type;

        public int Amount => _amount;

        public void DecreaseAmount(int value)
        {
            _amount = Math.Max(0, _amount - value);
        }

        public void IncreaseAmount(int value)
        {
            if (value > 0)
            {
                _amount += value;
            }
            else
            {
                throw new ArgumentException("Value must be positive");
            }
        }

        public bool IsRenewable => _renewable;

        public void Regenerate(double percentage, double time)
        {
            if (_renewable)
            {
                int regeneratedAmount = (int)(_maxAmount * percentage);
                // Aqui iria la logica para ir aumentando la cantidad poco a poco;
                // de momento se deja simple
                _amount = Math.Min(_amount + regeneratedAmount, _maxAmount);
            }
            else
            {
                throw new NotSupportedException("Resource is not renewable");
            }
        }

        public void SetResourceType(ResourceType value) { _type = value; }
        public void SetAmount(int value) { _amount = value; _maxAmount = value; }
        public void SetRenewable(bool value) { _renewable = value; }

        /// <summary>
        /// El tipo y el indicador de renovable quedan fuera por no ser numericos.
        /// </summary>
        private static readonly NumericFields<ResourceComponent> _fields =
            new NumericFields<ResourceComponent>()
                .Add("amount", c => c._amount, (c, v) => c.SetAmount((int)v));

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);

            if (values.ContainsKey("resourceType")) SetResourceType(Enum.Parse<ResourceType>(values["resourceType"].ToString(), true));
            if (values.ContainsKey("renewable")) SetRenewable(Convert.ToBoolean(values["renewable"]));
        }

        public override IComponent Clone()
        {
            return new ResourceComponent(_type, _amount, _renewable); // Copia el componente
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is ResourceComponent otherResource &&
                _type == otherResource._type &&
                _amount == otherResource._amount &&
                _renewable == otherResource._renewable &&
                _maxAmount == otherResource._maxAmount;
        }
    }
}
