using Core.ECS.Entity;

namespace Core
{
    /// <summary>
    /// Allows Core to request linking an entity with its engine representation
    /// (GameObject in Unity). The implementation lives in Unity/.
    /// </summary>
    public interface IEntityLinker
    {
        /// <summary>
        /// Links a Core entity with its engine representation.
        /// </summary>
        /// <param name="entity">Entity already created with pure Core components.</param>
        /// <param name="entityType">Logical type to resolve which visual resource to use.</param>
        void Link(IEntity entity, string entityType);
    }
}
