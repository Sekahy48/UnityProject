using System.Collections.Generic;
using Core.Events;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Gestiona y ejecuta los sistemas del juego.
    /// - Sistemas de juego periodicos: se ejecutan en cada tick de ClockSystem (les afectan la velocidad del tiempo, la pausa, etc.)
    /// - Sistemas de juego reactivos: se ejecutan al reaccionar a eventos concretos del juego
    /// - Sistemas del motor: se ejecutan cada fotograma con el deltaTime real (entrada, camara, interfaz)
    /// </summary>
    public class SystemManager
    {
        private readonly ClockSystem _clock = ClockSystem.GetInstance();
        private readonly EntityManager _entityManager;

        private readonly List<IPeriodicSystem> _gamePeriodicSystems = new();
        private readonly List<IPeriodicSystem> _engineSystems = new();
        private readonly List<IReactiveSystem> _gameReactiveSystems = new();

        private int _pendingTicks = 0;

        public SystemManager(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _clock.Attach(new TickCounter(this));
        }

        public SystemManager RegisterPeriodicGameSystem(IPeriodicSystem system)
        {
            _gamePeriodicSystems.Add(system);
            return this;
        }

        public SystemManager RegisterEngineSystem(IPeriodicSystem system)
        {
            _engineSystems.Add(system);
            return this;
        }

        public SystemManager RegisterReactiveGameSystem(IReactiveSystem system)
        {
            _gameReactiveSystems.Add(system);
            foreach (GameEventType type in system.SubscribedEvents)
                EventBus.GetInstance().Subscribe(type, system);
            return this;
        }

        /// <summary>
        /// Se llama cada fotograma desde GameMain.Update().
        /// </summary>
        public void Update(float deltaTime)
        {
            // Los sistemas del motor se ejecutan cada fotograma con el deltaTime real
            foreach (var system in _engineSystems)
                system.Process(deltaTime, _entityManager);

            // ClockSystem acumula tiempo y genera ticks
            _clock.Update(deltaTime);

            // Los sistemas de juego se ejecutan una vez por cada tick acumulado
            float tickTime = _clock.GetTickTime();
            while (_pendingTicks > 0)
            {
                _pendingTicks--;
                foreach (var system in _gamePeriodicSystems)
                    system.Process(tickTime, _entityManager);
            }
        }

        /// <summary>
        /// Devuelve un sistema registrado por su tipo. Util para conectar observadores.
        /// </summary>
        public T GetPeriodicSystem<T>() where T : class, IPeriodicSystem
        {
            foreach (var system in _gamePeriodicSystems)
                if (system is T typed) return typed;
            foreach (var system in _engineSystems)
                if (system is T typed) return typed;
            return null;
        }

        public T GetReactiveSystem<T>() where T : class, IReactiveSystem
        {
            foreach (var system in _gameReactiveSystems)
                if (system is T typed) return typed;
            return null;
        }

        /// <summary>
        /// Adaptador interno que cuenta los ticks de ClockSystem como observador.
        /// </summary>
        private class TickCounter : Observer.IObserver
        {
            private readonly SystemManager _owner;
            public TickCounter(SystemManager owner) { _owner = owner; }
            public void Update() { _owner._pendingTicks++; }
        }
    }
}
