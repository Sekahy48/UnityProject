using System.Collections.Generic;
using ECS.Entity;

namespace Inventory
{
    public class ItemDatabase
    {
        private static ItemDatabase _instance;
        private Dictionary<string, ItemEntity> _items = new Dictionary<string, ItemEntity>();

        private ItemDatabase()
        {
            // Load all base items
            // ...
        }

        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null) _instance = new ItemDatabase();
                return _instance;
            }
        }

        public ItemEntity GetItem(string id)
        {
            return _items[id];
        }
    }

}