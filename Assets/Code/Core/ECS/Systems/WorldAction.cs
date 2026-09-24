namespace Core.ECS.Systems
{
    /// <summary>
    /// Lo que se puede hacer con algo que esta en el mundo.
    ///
    /// <para>Es el equivalente de <c>ItemAction</c> para objetivos del mundo en vez de para
    /// items de un inventario. Son listas distintas porque las preguntas son distintas —a un
    /// monton del suelo no se le puede "transferir rapido" ni "dividir"— pero el patron es
    /// el mismo: el dominio contesta que acciones hay y la capa de interfaz solo las
    /// pinta.</para>
    /// </summary>
    public enum WorldAction
    {
        /// <summary>Llevarselo al inventario.</summary>
        PickUp,

        /// <summary>Ver que hay dentro sin cogerlo.</summary>
        Inspect
    }
}
