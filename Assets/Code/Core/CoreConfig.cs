namespace Core
{
    public static class CoreConfig
    {
        public static string BasePath { get; set; }
        public static string MappingPath => BasePath + "/id_mapping.json";
        public static string CatalogPath => BasePath + "/data.json";

        /// <summary>
        /// Ruta completa de un recurso que el catalogo nombra en relativo, como
        /// <c>images/espada.png</c> o <c>models/136_arcon_00.glb</c>.
        ///
        /// El catalogo guarda rutas relativas a proposito: quien lo exporto no sabe donde va
        /// a acabar la carpeta de datos, y una ruta absoluta del equipo del autor no
        /// significa nada en el equipo del jugador.
        /// </summary>
        public static string ResolveAsset(string relativePath)
            => string.IsNullOrEmpty(relativePath) ? null : BasePath + "/" + relativePath;
    }
}
