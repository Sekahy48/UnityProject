using Core.ECS.Entity;
using Core.ECS.Component;

namespace Core.Events
{
    public class GameEvent
    {
        private readonly GameEventType _eventType;
        private readonly IEntity _entity;
        private readonly IComponent _component;

        public GameEvent(GameEventType eventType, IEntity entity, IComponent component)
        {
            _eventType = eventType;
            _entity = entity;
            _component = component;
        }

        public GameEventType GetEventType()
        {
            return _eventType;
        }

        public IEntity GetEntity()
        {
            return _entity;
        }

        public T GetComponent<T>() where T : class, IComponent => _component as T;

    }
}