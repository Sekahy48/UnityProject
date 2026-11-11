namespace MVC.View
{
    /// <summary>
    /// Rutas de las imagenes de UI, relativas a StreamingAssets (TextureCache antepone la
    /// carpeta). Estan aqui y no en CoreConfig porque son arte de presentacion: Core no
    /// sabe que existen iconos.
    /// </summary>
    public static class UIImages
    {
        private const string ROOT = "images/";

        public const string EmptyIcon = ROOT + "empty-icon.png";

        /// <summary>Hueco vacio de un slot de equipo, por convencion de nombre.</summary>
        public static string Empty(string slotName) => ROOT + "slots/" + slotName + ".png";
    }
}