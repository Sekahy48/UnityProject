using ECS.Systems;

namespace Core.Contexts
{
    /// <summary>
    /// Contexto de datos del mundo: entidades, mapas, catálogos.
    /// Equivalente a DataContext en StackGo.
    /// </summary>
    public class GameDataContext
    {
        public EntityManager EntityManager { get; }

        public GameDataContext(EntityManager entityManager)
        {
            EntityManager = entityManager;
        }
    }
}
