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
        public IComponent Clone();
        public bool Equivalent(IComponent other);
    }
}