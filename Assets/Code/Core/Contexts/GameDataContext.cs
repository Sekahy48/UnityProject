using ECS.Systems;
using Item;

namespace Core.Contexts
{
    /// <summary>
    /// World data context: entities, maps, catalogs.
    /// Equivalent to DataContext in StackGo.
    /// </summary>
    public class GameDataContext
    {
        public EntityManager _entityManager { get; }
        public ItemCatalogue _itemCatalogue {get; }

        public GameDataContext(EntityManager entityManager, ItemCatalogue itemCatalogue)
        {
            _entityManager = entityManager;
            _itemCatalogue = itemCatalogue;
        }
    }
}
