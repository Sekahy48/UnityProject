using Core.Events;

namespace Core.Observer
{
    public interface IEventObserver
    { 
        public void UpdateOnEvent(GameEvent gameEvent);
    }
}