using System;

namespace ECS.Systems
{
    /// <summary>
    /// Interface for game loop systems.
    /// Each system processes entities that have the components it's interested in.
    /// </summary>
    public interface IPeriodicSystem
    {
        /// <summary>
        /// Processes a tick. The system queries EntityManager to get the relevant entities.
        /// </summary>
        /// <param name="deltaTime">Tick time (ClockSystem's tickTime for game systems, real deltaTime for engine systems)</param>
        /// <param name="entityManager">Access to the game's entities</param>
        void Process(float deltaTime, EntityManager entityManager);
    }
}
