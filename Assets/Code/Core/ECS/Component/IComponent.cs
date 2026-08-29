namespace ECS.Component
{
    /// <summary>
    /// Interface for entity components
    /// </summary>

    public interface IComponent
    {
        /// <summary>
        /// Deep clones the component.
        /// </summary>
        public IComponent Clone();
        public bool Equivalent(IComponent other);
    }
}