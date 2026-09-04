using System;
using System.Collections.Generic;

namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Entrada de un menu contextual. Es un Composite, igual que el arbol de inventario:
    /// una HOJA lleva la accion que se ejecuta al pulsarla, y una RAMA lleva las entradas
    /// que despliega al pasar por encima. Nunca las dos cosas.
    ///
    /// Esa exclusion no se comprueba, se hace inconstruible: hay un constructor por forma,
    /// asi que no existe manera de escribir una opcion con handler y con hijos a la vez.
    /// </summary>
    public readonly struct MenuOption
    {
        public readonly string OptionName;

        /// <summary>Accion a ejecutar. Null en las ramas.</summary>    
        public readonly Action<MenuInputs> Handler;

        /// <summary>Entradas que despliega. Null en las hojas.</summary>
        public readonly IReadOnlyList<MenuOption> SubOptions;

        /// <summary>
        /// Fields that will be recieved to execute the handler
        /// </summary>
        public readonly IReadOnlyList<MenuField> Fields;

        /// <summary>Hoja: una opcion final que hace algo al pulsarla.</summary>
        public MenuOption(string optionName, Action<MenuInputs> handler, IReadOnlyList<MenuField> fields)
        {
            if (string.IsNullOrEmpty(optionName)) throw new ArgumentNullException(nameof(optionName));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            OptionName = optionName;
            Handler = handler;
            SubOptions = null;
            Fields = fields ?? Array.Empty<MenuField>();
        }

        /// <summary>
        /// Rama: una opcion que solo despliega otras. Se exige que traiga al menos una,
        /// porque una rama vacia es una entrada muerta en el menu — quien la construye
        /// debe omitirla, no crearla sin hijos.
        /// </summary>
        public MenuOption(string optionName, IReadOnlyList<MenuOption> subOptions)
        {
            if (string.IsNullOrEmpty(optionName)) throw new ArgumentNullException(nameof(optionName));
            if (subOptions == null) throw new ArgumentNullException(nameof(subOptions));
            if (subOptions.Count == 0)
                throw new ArgumentException("Cannot create a branch MenuOption with no children.",
                                            nameof(subOptions));

            OptionName = optionName;
            Handler = null;
            SubOptions = subOptions;
            Fields = Array.Empty<MenuField>();
        }

        public bool IsLeaf => Handler != null;
    }
}