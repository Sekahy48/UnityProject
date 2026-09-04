using System.Collections.Generic;
using Core.Events;
using Core.Observer;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Interface for game reactive systems.
    /// Each system does operations in reaction to game events.
    /// </summary>
    public interface IReactiveSystem : IEventObserver
    {
        IEnumerable<GameEventType> SubscribedEvents { get; }
    }
}
