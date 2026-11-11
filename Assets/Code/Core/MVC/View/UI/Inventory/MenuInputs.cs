using System;
using System.Collections.Generic;

namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Valores que la vista ha recogido de los <see cref="MenuField"/> de una opcion,
    /// listos para su handler. Se construye una vez, al pulsar, y no se modifica despues.
    /// </summary>
    public class MenuInputs
    {
        /// <summary>Para opciones sin campos: evita repartir nulos por los handlers.</summary>
        public static readonly MenuInputs Empty = new MenuInputs(new Dictionary<string, object>());

        private readonly IReadOnlyDictionary<string, object> _values;

        public MenuInputs(IReadOnlyDictionary<string, object> values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public int GetInt(string id) => Get<int>(id);
        public float GetFloat(string id) => Get<float>(id);
        public string GetText(string id) => Get<string>(id);

        public bool Has(string id) => _values.ContainsKey(id);

        private T Get<T>(string id)
        {
            if (!_values.TryGetValue(id, out object value))
                throw new KeyNotFoundException(
                    $"MenuInputs: no existe el campo '{id}'. Declaralo como MenuField en la MenuOption.");

            if (value is not T typed)
                throw new InvalidCastException(
                    $"MenuInputs: el campo '{id}' contiene {value?.GetType().Name ?? "null"}, no {typeof(T).Name}.");

            return typed;
        }
    }
}