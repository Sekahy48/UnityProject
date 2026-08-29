using ECS.Entity;
using ECS.Component;

namespace Events
{
    public class GameEvent
    {
        private readonly GameEventType eventType;
        private readonly IEntity entity;
        private readonly IComponent component;

        public GameEvent(GameEventType eventType, IEntity entity, IComponent component)
        {
            this.eventType = eventType;
            this.entity = entity;
            this.component = component;
        }

        public GameEventType GetEventType()
        {
            return eventType;
        }

        public IEntity GetEntity()
        {
            return entity;
        }

        public T GetComponent<T>() where T : class, IComponent => component as T;

    }
}