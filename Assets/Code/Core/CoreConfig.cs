namespace Core
{
    public static class CoreConfig
    {
        public static string BasePath { get; set; }
        public static string MappingPath => BasePath + "/id_mapping.json";
        public static string CatalogPath => BasePath + "/data.json";
    }
}
