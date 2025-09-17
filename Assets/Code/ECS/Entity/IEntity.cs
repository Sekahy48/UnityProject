using System;
using ECS.Component;
using Handler;

namespace ECS.Entity
{
    public interface IEntity
    {
        /// <summary>
        /// Devuelve el id (handler)
        /// </summary>
        Handler.IHandler GetId();

        /// <summary>
        /// Devuelve el id como entero
        /// </summary>
        int GetIdAsInt();

        /// <summary>
        /// Devuelve el tipo (handler)
        /// </summary>
        Handler.IHandler GetEntityType();

        /// <summary>
        /// Nombre IDENTITARIO de la entidad
        /// </summary>
        string GetName();

        /// <summary>
        /// Devuelve el identificador compuesto (tipo + id)
        /// </summary>
        Handler.IHandler GetCompoundIdentification();

        /// <summary>
        /// Añade un componente a la entidad
        /// </summary>
        void AddComponent<T>(T component) where T : IComponent;

        /// <summary>
        /// Obtiene un componente por nombre
        /// </summary>
        T GetComponent<T>(Type target) where T : IComponent;

        /// <summary>
        /// Elimina un componente por nombre
        /// </summary>
        bool RemoveComponent(Type target);

        /// <summary>
        /// Comprueba si existe el componente
        /// </summary>
        bool HasComponent(Type target);

        /// <summary>
        /// Clona la entidad (nuevo ID)
        /// </summary>
        IEntity Clone(); // override del Clone() de ICloneable
    }
}
