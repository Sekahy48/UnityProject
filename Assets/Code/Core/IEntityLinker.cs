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
        /// <param name="entityType">Archetype, used to decide whether the engine object is
        /// found in the scene or created on the spot.</param>
        void Link(IEntity entity, EntityType entityType);

        /// <summary>
        /// Deshace <see cref="Link"/>: destruye la representacion de la entidad en el motor
        /// y le quita el componente puente, de modo que la entidad queda como si nunca se
        /// hubiera enlazado.
        ///
        /// <para>Quitar el componente no es limpieza cosmetica. Su presencia es el unico
        /// hecho que dice "esta entidad tiene cuerpo en el motor"; si sobreviviera al
        /// objeto, cualquiera que aun guarde la entidad —un objetivo cacheado, un panel
        /// abierto— creeria que puede tocar algo que ya no existe.</para>
        ///
        /// <para>No distingue entre lo que <c>Link</c> encontro en la escena y lo que creo:
        /// destruye siempre. No esta pensado para el jugador; su muerte, si llega a
        /// necesitar esto, se diseña cuando toque.</para>
        ///
        /// <para>No saca la entidad de <c>EntityManager</c>: eso es de Core y lo hace quien
        /// llama, igual que quien llama a <c>Link</c> es quien la creo.</para>
        /// </summary>
        /// <param name="entity">Entidad enlazada. Si no lo esta, no hace nada.</param>
        void Unlink(IEntity entity);
    }
}
