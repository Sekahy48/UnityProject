using ECS.Systems;

namespace Core.Contexts
{
    /// <summary>
    /// World data context: entities, maps, catalogs.
    /// Equivalent to DataContext in StackGo.
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
