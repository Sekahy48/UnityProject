namespace Core.MVC.View.UI.Radial
{
    /// <summary>
    /// Una opcion ya resuelta, para el radial y para la marca de la E. La vista no sabe de
    /// acciones ni de inventarios: pinta lo que le llega.
    /// </summary>
    public class RadialOption
    {
        /// <summary>Texto principal ("Coger", "Coger x3", "Principal (Cofre)").</summary>
        public string Label;

        /// <summary>Texto bajo la etiqueta ("No hay espacio suficiente"), o null.</summary>
        public string Hint;

        /// <summary>Abre un anillo; la vista anade "[Mas opciones]" debajo.</summary>
        public bool HasChildren;

        /// <summary>En gris; elegirla no hace nada.</summary>
        public bool Disabled;

        public bool SameAs(RadialOption other)
            => other != null
               && other.Label == Label
               && other.Hint == Hint
               && other.HasChildren == HasChildren
               && other.Disabled == Disabled;
    }
}
