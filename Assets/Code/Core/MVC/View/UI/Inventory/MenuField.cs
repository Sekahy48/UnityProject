using System;

namespace Core.MVC.View.UI.Inventory
{   
    /// <summary>
    /// Campo editable que acompaña a una opcion de menu (la cantidad a tirar, por ejemplo).
    /// Describe QUE pedir, no como pintarlo: la vista elige el widget segun <see cref="Type"/>.
    /// Se construye con las fabricas estaticas, no con new.
    /// </summary>
    public readonly struct MenuField
    {
        /// <summary>Clave con la que el handler recupera el valor desde MenuInputs.</summary>
        public readonly string Id;

        /// <summary>Texto a mostrar junto al campo. Null para no mostrar ninguno.</summary>
        public readonly string Label;

        public readonly MenuFieldType Type;

        /// <summary>Limites para los tipos numericos. Sin uso en Text.</summary>
        public readonly float Min, Max;

        /// <summary>Valor inicial de los tipos numericos. Sin uso en Text.</summary>
        public readonly float DefaultNumber;

        /// <summary>Valor inicial de Text. Null en los tipos numericos.</summary>
        public readonly string DefaultText;

        private MenuField(string id, string label, MenuFieldType type,
                          float min, float max, float defaultNumber, string defaultText)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            Id = id;
            Label = label;
            Type = type;
            Min = min;
            Max = max;
            DefaultNumber = defaultNumber;
            DefaultText = defaultText;
        }

        /// <summary>
        /// Campo entero. Sin <paramref name="defaultValue"/> arranca en el maximo, que en un
        /// campo de cantidad significa "todo": pulsar sin tocar nada hace el gesto habitual.
        /// </summary>
        public static MenuField Int(string id, int max, int min = 1,
                                    int? defaultValue = null, string label = null)
        {
            if (max < min)
                throw new ArgumentException($"max ({max}) menor que min ({min}).", nameof(max));

            int initial = Math.Clamp(defaultValue ?? max, min, max);
            return new MenuField(id, label, MenuFieldType.Int, min, max, initial, null);
        }

        public static MenuField Float(string id, float min, float max,
                                      float defaultValue, string label = null)
        {
            if (max < min)
                throw new ArgumentException($"max ({max}) menor que min ({min}).", nameof(max));

            return new MenuField(id, label, MenuFieldType.Float,
                                 min, max, Math.Clamp(defaultValue, min, max), null);
        }

        public static MenuField Text(string id, string defaultText = "", string label = null)
            => new MenuField(id, label, MenuFieldType.Text, 0f, 0f, 0f, defaultText ?? "");
    }
    
    public enum MenuFieldType
    {
        Int,
        Float,
        Text
    }

    
}