using Events;

namespace Observer
{
    public interface IEventObserver
    { 
        public void UpdateOnEvent(GameEvent gameEvent);
    }
}