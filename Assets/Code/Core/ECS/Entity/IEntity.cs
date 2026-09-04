using System; 
using Core.ECS.Component;
using Core.Handler;

namespace Core.ECS.Entity
{
    public interface IEntity
    {
        /// <summary>
        /// Returns the id (handler)
        /// </summary>
        Handler.IHandler GetId();

        /// <summary>
        /// Returns the id as an integer
        /// </summary>
        int GetIdAsInt();

        /// <summary>
        /// Returns the type (handler)
        /// </summary>
        Handler.IHandler GetEntityType();

        /// <summary>
        /// Identifying name of the entity
        /// </summary>
        string GetName();

        /// <summary>
        /// Returns the compound identifier (type + id)
        /// </summary>
        Handler.IHandler GetCompoundIdentification();

        /// <summary>
        /// Adds a component to the entity
        /// </summary>
        void AddComponent<T>(T component) where T : IComponent;

        /// <summary>
        /// Gets a component by generic type
        /// </summary>
        T GetComponent<T>() where T : IComponent;

        /// <summary>
        /// Gets a component by dynamic Type (for when the type isn't known at compile time)
        /// </summary>
        IComponent GetComponentByType(Type target);

        /// <summary>
        /// Removes a component by name
        /// </summary>
        bool RemoveComponent(Type target);

        /// <summary>
        /// Checks if the component exists
        /// </summary>
        bool HasComponent(Type target);

        /// <summary>
        /// Clones the entity (new ID)
        /// </summary>
        IEntity Clone(); // override of ICloneable's Clone()

        /// <summary>
        /// Checks if this entity is equivalent to another (same type and same components with same values)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equivalent(IEntity other);
    }
}
