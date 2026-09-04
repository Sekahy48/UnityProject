using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Inventory;

namespace Core.Contexts
{
    /// <summary>
    /// Session context: state of the current game session.
    /// Equivalent to SessionContext in StackGo.
    /// </summary>
    public class GameSessionContext
    {
        public IEntity _player { get; private set; }
        public ItemEntity _firstInventorySrc { get; private set; }
        public ItemEntity _secondInventorySrc { get; private set; }

        public ClockSystem Clock => ClockSystem.GetInstance();

        public void SetPlayer(IEntity player)
        {
            _player = player;
        }

        public void SetFirstInventorySrc(ItemEntity inventoySrc)
        {
            _firstInventorySrc = inventoySrc;
            _firstInventorySrc.AddComponent(new InventoryComponent( new InventoryObject(_firstInventorySrc)));
        }

            public void SetSecondInventorySrc(ItemEntity inventoySrc)
            {
                _secondInventorySrc = inventoySrc;
                _secondInventorySrc.AddComponent(new InventoryComponent( new InventoryObject(_secondInventorySrc)));
            }
    }
}
