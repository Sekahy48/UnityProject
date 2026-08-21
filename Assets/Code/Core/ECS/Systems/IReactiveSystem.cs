using System.Collections.Generic;
using Events;
using Observer;

namespace ECS.Systems
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
