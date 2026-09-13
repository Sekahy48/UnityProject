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

        /// <remarks>El inventario no se monta aqui: viene ya puesto desde
        /// <see cref="Item.ItemCatalogue.CreateItem"/>, que se lo da a todo lo que declare
        /// StorageComponent. Anadirlo tambien aqui lo sustituiria por uno vacio.</remarks>
        public void SetFirstInventorySrc(ItemEntity inventoySrc)
        {
            _firstInventorySrc = inventoySrc;
        }

        public void SetSecondInventorySrc(ItemEntity inventoySrc)
        {
            _secondInventorySrc = inventoySrc;
        }
    }
}
