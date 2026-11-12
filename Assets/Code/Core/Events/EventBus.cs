using System;
using System.Collections.Generic;
using Core.Observer;

namespace Core.Events
{
    /// <summary>
    /// Centralized event bus. Publish/Subscribe for discrete game events.
    /// </summary>
    public class EventBus
    {
        private static EventBus _instance;

        private readonly Dictionary<GameEventType, List<IEventObserver>> _subscribers = new();

        private EventBus() { }

        public static EventBus GetInstance()
        {
            if (_instance == null)
                _instance = new EventBus();
            return _instance;
        }

        public void Subscribe(GameEventType eventType, IEventObserver observer)
        {
            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<IEventObserver>();

            if (!_subscribers[eventType].Contains(observer))
                _subscribers[eventType].Add(observer);
        }

        public void Unsubscribe(GameEventType eventType, IEventObserver observer)
        {
            if (_subscribers.TryGetValue(eventType, out var list))
                list.Remove(observer);
        }

        public void Post(GameEvent gameEvent)
        {
            if (!_subscribers.TryGetValue(gameEvent.GetEventType(), out var list)) return;

            // Copia: un observador puede suscribirse o darse de baja al reaccionar,
            // y modificar la lista mientras se recorre lanza InvalidOperationException.
            foreach (var observer in new List<IEventObserver>(list))
            {
                try
                {
                    observer.UpdateOnEvent(gameEvent);
                }
                catch (Exception e)
                {
                    CoreLogger.Instance.LogError(
                        $"EventBus: el observador {observer.GetType().Name} fallo al procesar " +
                        $"{gameEvent.GetEventType()}: {e}");
                }
            }
        }

        /// <summary>
        /// Clears all subscriptions. Useful for tests or scene changes.
        /// </summary>
        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}
