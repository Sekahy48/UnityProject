using System;
using System.Collections.Generic;
using Observer;

namespace Events
{
    /// <summary>
    /// Centralized event bus. Publish/Subscribe for discrete game events.
    /// </summary>
    public class EventBus
    {
        private static EventBus instance;

        private readonly Dictionary<GameEventType, List<IEventObserver>> subscribers = new();

        private EventBus() { }

        public static EventBus GetInstance()
        {
            if (instance == null)
                instance = new EventBus();
            return instance;
        }

        public void Subscribe(GameEventType eventType, IEventObserver observer)
        {
            if (!subscribers.ContainsKey(eventType))
                subscribers[eventType] = new List<IEventObserver>();

            if (!subscribers[eventType].Contains(observer))
                subscribers[eventType].Add(observer);
        }

        public void Unsubscribe(GameEventType eventType, IEventObserver observer)
        {
            if (subscribers.TryGetValue(eventType, out var list))
                list.Remove(observer);
        }

        public void Post(GameEvent gameEvent)
        {
            if (subscribers.TryGetValue(gameEvent.GetEventType(), out var list))
            {
                foreach (var observer in list)
                    observer.UpdateOnEvent(gameEvent);
            }
        }

        /// <summary>
        /// Clears all subscriptions. Useful for tests or scene changes.
        /// </summary>
        public void Clear()
        {
            subscribers.Clear();
        }
    }
}
