using Core.ECS.Systems;
using Core.Item;
using Core.ECS;
using AC = Core.Utils.ArgumentChecker;

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
            AC.CheckNotNull(entityManager, nameof(entityManager));
            AC.CheckNotNull(itemCatalogue, nameof(itemCatalogue));
            _entityManager = entityManager;
            _itemCatalogue = itemCatalogue;
        }
    }
}
