using System.Collections.Generic;

namespace ECS.Systems
{
    /// <summary>
    /// Manages and runs the game's systems.
    /// - Game systems: run on every ClockSystem tick (affected by timeSpeed, pause, etc.)
    /// - Engine systems: run every frame with real deltaTime (input, camera, UI)
    /// </summary>
    public class SystemManager
    {
        private readonly ClockSystem clock = ClockSystem.GetInstance();
        private readonly EntityManager entityManager;

        private readonly List<IGameSystem> gameSystems = new();
        private readonly List<IGameSystem> engineSystems = new();

        private int pendingTicks = 0;

        public SystemManager(EntityManager entityManager)
        {
            this.entityManager = entityManager;
            clock.Attach(new TickCounter(this));
        }

        public void RegisterGameSystem(IGameSystem system)
        {
            gameSystems.Add(system);
        }

        public void RegisterEngineSystem(IGameSystem system)
        {
            engineSystems.Add(system);
        }

        /// <summary>
        /// Call every frame from GameMain.Update().
        /// </summary>
        public void Update(float deltaTime)
        {
            // Engine systems run every frame with real deltaTime
            foreach (var system in engineSystems)
                system.Process(deltaTime, entityManager);

            // ClockSystem accumulates time and generates ticks
            clock.Update(deltaTime);

            // Game systems run once per accumulated tick
            float tickTime = clock.GetTickTime();
            while (pendingTicks > 0)
            {
                pendingTicks--;
                foreach (var system in gameSystems)
                    system.Process(tickTime, entityManager);
            }
        }

        /// <summary>
        /// Gets a registered system by type. Useful for connecting observers.
        /// </summary>
        public T GetGameSystem<T>() where T : class, IGameSystem
        {
            foreach (var system in gameSystems)
                if (system is T typed) return typed;
            foreach (var system in engineSystems)
                if (system is T typed) return typed;
            return null;
        }

        /// <summary>
        /// Internal adapter to count ClockSystem ticks via observer.
        /// </summary>
        private class TickCounter : Observer.IObserver
        {
            private readonly SystemManager owner;
            public TickCounter(SystemManager owner) { this.owner = owner; }
            public void Update() { owner.pendingTicks++; }
        }
    }
}
