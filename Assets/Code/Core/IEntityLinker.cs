using ECS.Entity;

namespace Core
{
    /// <summary>
    /// Permite a Core solicitar que una entidad se vincule con su representación
    /// en el motor (GameObject en Unity). La implementación vive en Unity/.
    /// </summary>
    public interface IEntityLinker
    {
        /// <summary>
        /// Vincula una entidad Core con su representación en el motor.
        /// </summary>
        /// <param name="entity">Entidad ya creada con componentes Core puros.</param>
        /// <param name="entityType">Tipo lógico para resolver qué recurso visual usar.</param>
        void Link(IEntity entity, string entityType);
    }
}
