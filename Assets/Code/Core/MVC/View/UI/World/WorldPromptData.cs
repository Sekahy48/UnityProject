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
        /// <summary>La accion de la pulsacion corta, ya con su nombre ("Recoger").</summary>
        public string ActionLabel;

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
               && other.ActionLabel == ActionLabel
               && other.TargetLabel == TargetLabel
               && other.HasMoreActions == HasMoreActions;
    }
}
