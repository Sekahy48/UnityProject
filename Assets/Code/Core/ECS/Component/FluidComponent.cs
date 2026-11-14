using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{
    public class FluidComponent : BasicComponent, IJsonLoadable, INumericFields
    {
        private Dictionary<ResourceType, float> _fluids;
        private float _maxCapacity;

        public FluidComponent()
        {
            _fluids = new Dictionary<ResourceType, float>();
        }

        public FluidComponent(float maxCapacity)
        {
            _fluids = new Dictionary<ResourceType, float>();
            _maxCapacity = maxCapacity;
            Name = "FluidComponent";
        }

        // Anade fluido al componente
        public float AddFluid(ResourceType fluid, float amount)
        {
            float left = GetSpaceLeft();

            if (left <= 0)
                return -1;

            float toAdd = Math.Min(amount, left);

            if (_fluids.ContainsKey(fluid))
                _fluids[fluid] += toAdd;
            else
                _fluids[fluid] = toAdd;

            return amount - toAdd;
        }

        // Saca fluido del componente
        public float DrainFluid(ResourceType fluid, float amount)
        {
            float outAmount = 0;

            if (_fluids.ContainsKey(fluid))
            {
                float left = _fluids[fluid];
                if (amount >= left)
                {
                    _fluids[fluid] = 0f;
                    outAmount = left;
                }
                else
                {
                    _fluids[fluid] = left - amount;
                    outAmount = amount;
                }
            }
            return outAmount;
        }

        // Capacidad que queda libre
        public float GetSpaceLeft()
        {
            float content = 0;
            foreach (var amount in _fluids.Values)
            {
                content += amount;
            }

            if (_maxCapacity < content)
            {
                return -1;
            }

            return _maxCapacity - content;
        }

        public void SetMaxCapacity(float value) { _maxCapacity = value; }

        /// <summary>
        /// El contenido actual no se declara porque es un diccionario por tipo de fluido y
        /// no un numero suelto; si algun dia hace falta preguntarlo sera con un nombre
        /// propio, como <c>fill</c>, y decidiendo que significa.
        /// </summary>
        private static readonly NumericFields<FluidComponent> _fields =
            new NumericFields<FluidComponent>()
                .Add("maxCapacity", c => c._maxCapacity, (c, v) => c.SetMaxCapacity(v));

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);
        }

        public override IComponent Clone()
        {
            FluidComponent copy = new FluidComponent(_maxCapacity);

            // Copia profunda del mapa de fluidos
            foreach (var entry in _fluids)
            {
                copy._fluids[entry.Key] = entry.Value;
            }

            return copy;
        }

        /// <summary>
        /// Si este componente tiene exactamente el mismo contenido que otro componente de
        /// fluido, sin importar el orden de los fluidos.
        /// </summary>
        /// <param name="other"> Otro componente de fluido con el que comparar.</param>
        /// <returns>True si los dos tienen el mismo contenido, false si no.</returns>
        public Boolean SameContent(FluidComponent other)
        {
            if (_fluids.Count != other._fluids.Count) return false;

            foreach (KeyValuePair<ResourceType, float> entry in _fluids)
            {
                if (!other._fluids.TryGetValue(entry.Key, out float otherValue))
                    return false;
                if (Math.Abs(otherValue - entry.Value) > 0.001f)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Si este componente contiene, como minimo, el mismo contenido que otro componente de
        /// fluido: la misma cantidad o mas de cada tipo de fluido presente en el otro,
        /// sin importar el orden de los fluidos.
        ///
        /// </summary>
        /// <param name="other"> Otro componente de fluido con el que comparar.</param>
        /// <returns>True si este contiene al menos el contenido del otro, false si no.</returns>
        public Boolean ContainsAtLeast(FluidComponent other)
        {
            if (_fluids.Count != other._fluids.Count) return false;

            foreach (KeyValuePair<ResourceType, float> entry in _fluids)
            {
                if (!other._fluids.TryGetValue(entry.Key, out float otherValue))
                    return false;
                if (otherValue - entry.Value > 0.0f)
                    return false;
            }
            return true;
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is FluidComponent otherFluid &&
                _maxCapacity == otherFluid._maxCapacity &&
                SameContent(otherFluid);

        }
    }
}
