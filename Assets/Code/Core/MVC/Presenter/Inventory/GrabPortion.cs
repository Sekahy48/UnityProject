namespace Core.MVC.Presenter.Inventory
{
    /// <summary>
    /// Parte de una pila sobre la que actua un gesto. Es la intencion del jugador, no la tecla
    /// que la produjo: la vista traduce el modificador a esto y Core nunca sabe de teclados.
    /// </summary>
    public enum GrabPortion
    {
        All,
        Half,
        One
    }

    public static class GrabPortionExtensions
    {
        /// <summary>
        /// Unidades que representa esta porcion sobre una cantidad disponible. La mitad
        /// redondea hacia arriba: al partir un numero impar, quien hace el gesto se queda la
        /// parte grande.
        /// </summary>
        public static int UnitsOf(this GrabPortion portion, int available)
        {
            if (available <= 0) return 0;

            switch (portion)
            {
                case GrabPortion.One:  return 1;
                case GrabPortion.Half: return (available + 1) / 2;
                default:               return available;
            }
        }
    }
}
