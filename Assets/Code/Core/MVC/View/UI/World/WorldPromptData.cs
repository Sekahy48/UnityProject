using Core.MVC.View.UI.Radial;

namespace Core.MVC.View.UI.World
{
    /// <summary>
    /// Lo que dice la marca de interaccion sobre un objetivo del mundo.
    ///
    /// Solo texto ya resuelto: la vista no sabe que es un monton ni que acciones existen,
    /// igual que la tira de inspeccion recibe <c>ItemDisplayData</c> y no entidades.
    /// </summary>
    public class WorldPromptData
    {
        /// <summary>
        /// La accion de la pulsacion corta, ya resuelta: nombre, texto de debajo y si esta en
        /// gris. La misma clase que una porcion del radial, para que marca y menu no puedan
        /// contar cosas distintas.
        /// </summary>
        public RadialOption Option;

        /// <summary>Sobre que se hace ("Manzana x5"). Vacio si no hay nada que decir.</summary>
        public string TargetLabel;

        /// <summary>
        /// Hay mas acciones que la de la pulsacion corta: la vista avisa de que manteniendo
        /// hay mas. Se muestra aunque el menu radial aun no exista, para que el aviso no
        /// cambie de forma el dia que llegue.
        /// </summary>
        public bool HasMoreActions;

        public bool SameAs(WorldPromptData other)
            => other != null
               && other.Option != null && other.Option.SameAs(Option)
               && other.TargetLabel == TargetLabel
               && other.HasMoreActions == HasMoreActions;
    }
}
