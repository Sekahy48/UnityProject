namespace Core.ECS.Component
{
    /// <summary>
    /// Marca el componente que une una entidad de Core con su representacion en el motor.
    ///
    /// <para><b>No se clona.</b> Ese componente solo lo pone <c>IEntityLinker.Link</c> y solo
    /// lo quita <c>Unlink</c>. Si el clon de una entidad lo copiara, dos entidades
    /// apuntarian al mismo GameObject: al destruir una, la otra se quedaria sin cuerpo sin
    /// que nadie lo supiera, y la copia no seria equivalente al original solo por llevarlo.
    /// <c>InGameEntity.Clone</c> se lo salta.</para>
    ///
    /// <para>Interfaz de Core y no comprobacion del tipo concreto porque el tipo concreto
    /// vive en la capa de Unity, y Core no puede nombrarlo.</para>
    /// </summary>
    public interface IEngineBridge : IComponent
    {
    }
}
