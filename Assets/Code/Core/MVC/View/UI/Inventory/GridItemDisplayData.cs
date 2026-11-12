namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Un item junto con su sitio en la rejilla tetris.
    /// Compone ItemDisplayData en vez de heredar de el, para que las coordenadas de la
    /// rejilla no se cuelen en los contextos donde los items no tienen posicion
    /// (tira de inspeccion, subslots de equipo).
    /// </summary>
    public class GridItemDisplayData
    {
        public ItemDisplayData Item { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }

        /// <summary>
        /// Este nodo es el origen de lo que lleva la mano. Todavia no ha salido nada de la
        /// rejilla, asi que se sigue dibujando, atenuado, para mostrar de donde salieron las unidades.
        /// </summary>
        public bool IsGrabbed { get; set; }
    }
}
