namespace ECS.Component
{
    /// <summary>
    /// Interfaz para los componentes de las entidades
    /// </summary>
     
    public interface IComponent
    { 
        /// <summary>
        /// Clona "deep" el componente.
        /// </summary>
        IComponent Clone();
    }
}