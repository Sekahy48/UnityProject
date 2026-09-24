namespace Core.ECS.Entity
{
    /// <summary>
    /// Arquetipos de entidad que el juego sabe construir.
    ///
    /// Es la clave del registro de prototipos y el dato con el que la capa de motor decide
    /// que representacion darle a una entidad. Antes era una cadena suelta, con dos
    /// consecuencias: <c>CreateEntity("groundLot")</c> con una errata compilaba y fallaba en
    /// ejecucion, y el enlazador comparaba contra un literal repetido en otro fichero, de
    /// modo que renombrar un arquetipo y olvidarse de uno de los dos sitios dejaba todo
    /// compilando y la entidad sin representacion.
    ///
    /// <para>La lista es cerrada a proposito: estos arquetipos los construye
    /// <see cref="Core.Factories.PrototypeFactory"/> a mano, con componentes elegidos en
    /// codigo. No son los items, que si vienen del catalogo y se identifican por su nombre.
    /// Un <see cref="ItemEntity"/> no tiene arquetipo: ser un item lo dice su clase.</para>
    /// </summary>
    public enum EntityType
    {
        Player,
        ResourceNode,
        AliveEntity,
        GroundLot
    }
}
