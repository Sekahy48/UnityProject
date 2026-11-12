using System; 
using Core.ECS.Component;
using Core.Handler;

namespace Core.ECS.Entity
{
    public interface IEntity
    {
        /// <summary>
        /// Devuelve el id (manejador)
        /// </summary>
        Handler.IHandler GetId();

        /// <summary>
        /// Devuelve el id como entero
        /// </summary>
        int GetIdAsInt();

        /// <summary>
        /// Nombre identificativo de la entidad
        /// </summary>
        string GetName();

        /// <summary>
        /// Devuelve el identificador compuesto (tipo + id)
        /// </summary>
        Handler.IHandler GetCompoundIdentification();

        /// <summary>
        /// Anade un componente a la entidad
        /// </summary>
        void AddComponent<T>(T component) where T : IComponent;

        /// <summary>
        /// Devuelve un componente por tipo generico
        /// </summary>
        T GetComponent<T>() where T : IComponent;

        /// <summary>
        /// Devuelve un componente por Type dinamico (cuando el tipo no se conoce al compilar)
        /// </summary>
        IComponent GetComponentByType(Type target);

        /// <summary>
        /// Todos los componentes de la entidad. Lo necesita <see cref="Equivalent"/> para comparar
        /// los conjuntos de componentes en los dos sentidos, no en uno solo.
        /// </summary>
        System.Collections.Generic.IEnumerable<IComponent> GetComponents();

        /// <summary>
        /// Quita un componente por nombre
        /// </summary>
        bool RemoveComponent(Type target);

        /// <summary>
        /// Comprueba si el componente existe
        /// </summary>
        bool HasComponent(Type target);

        /// <summary>
        /// Clona la entidad (con id nuevo)
        /// </summary>
        IEntity Clone(); // redefine el Clone() de ICloneable

        /// <summary>
        /// Si esta entidad es equivalente a otra (mismo tipo y mismos componentes con los mismos valores)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equivalent(IEntity other);
    }
}
