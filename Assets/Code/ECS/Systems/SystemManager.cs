using System.Collections.Generic;

namespace ECS.Systems
{
    /// <summary>
    /// Gestiona y ejecuta los sistemas del juego.
    /// - Game systems: se ejecutan en cada tick de ClockSystem (afectados por timeSpeed, pausa, etc.)
    /// - Engine systems: se ejecutan cada frame con deltaTime real (input, cámara, UI)
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
        /// Llamar cada frame desde GameMain.Update().
        /// </summary>
        public void Update(float deltaTime)
        {
            // Engine systems corren cada frame con deltaTime real
            foreach (var system in engineSystems)
                system.Process(deltaTime, entityManager);

            // ClockSystem acumula tiempo y genera ticks
            clock.Update(deltaTime);

            // Game systems corren una vez por cada tick acumulado
            float tickTime = clock.GetTickTime();
            while (pendingTicks > 0)
            {
                pendingTicks--;
                foreach (var system in gameSystems)
                    system.Process(tickTime, entityManager);
            }
        }

        /// <summary>
        /// Obtiene un sistema registrado por tipo. Útil para conectar observers.
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
        /// Adaptador interno para contar ticks de ClockSystem via observer.
        /// </summary>
        private class TickCounter : Observer.IObserver
        {
            private readonly SystemManager owner;
            public TickCounter(SystemManager owner) { this.owner = owner; }
            public void Update() { owner.pendingTicks++; }
        }
    }
}
