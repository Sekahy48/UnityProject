using System.Collections.Generic;
using Core.Events;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Manages and runs the game's systems.
    /// - Game perodic systems: run on every ClockSystem tick (affected by timeSpeed, pause, etc.)
    /// - Game reactive systems: run reacting to concrete game events
    /// - Engine systems: run every frame with real deltaTime (input, camera, UI)
    /// </summary>
    public class SystemManager
    {
        private readonly ClockSystem clock = ClockSystem.GetInstance();
        private readonly EntityManager entityManager;

        private readonly List<IPeriodicSystem> gamePeriodicSystems = new();
        private readonly List<IPeriodicSystem> engineSystems = new();
        private readonly List<IReactiveSystem> gameReactiveSystems = new();

        private int pendingTicks = 0;

        public SystemManager(EntityManager entityManager)
        {
            this.entityManager = entityManager;
            clock.Attach(new TickCounter(this));
        }

        public SystemManager RegisterPeriodicGameSystem(IPeriodicSystem system)
        {
            gamePeriodicSystems.Add(system);
            return this;
        }

        public SystemManager RegisterEngineSystem(IPeriodicSystem system)
        {
            engineSystems.Add(system);
            return this;
        }

        public SystemManager RegisterReactiveGameSystem(IReactiveSystem system)
        {
            gameReactiveSystems.Add(system);
            foreach (GameEventType type in system.SubscribedEvents)
                EventBus.GetInstance().Subscribe(type, system);
            return this;
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
                foreach (var system in gamePeriodicSystems)
                    system.Process(tickTime, entityManager);
            }
        }

        /// <summary>
        /// Gets a registered system by type. Useful for connecting observers.
        /// </summary>
        public T GetPeriodicSystem<T>() where T : class, IPeriodicSystem
        {
            foreach (var system in gamePeriodicSystems)
                if (system is T typed) return typed;
            foreach (var system in engineSystems)
                if (system is T typed) return typed;
            return null;
        }

        public T GetReactiveSystem<T>() where T : class, IReactiveSystem
        {
            foreach (var system in gameReactiveSystems)
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
