
using Inventory;

namespace Core.Contexts
{
    public class GameInteractionContext
    {
        public HandBuffer _handBuffer { get; }

        public GameInteractionContext()
        {
            _handBuffer = new HandBuffer();
        }
    }
}