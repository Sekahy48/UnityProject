using System;

namespace ECS.Systems
{
    /// <summary>
    /// Interfaz para sistemas del game loop.
    /// Cada sistema procesa entidades que tengan los componentes que le interesan.
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>
        /// Procesa un tick. El sistema consulta EntityManager para obtener las entidades relevantes.
        /// </summary>
        /// <param name="deltaTime">Tiempo del tick (tickTime de ClockSystem para game systems, deltaTime real para engine systems)</param>
        /// <param name="entityManager">Acceso a las entidades del juego</param>
        void Process(float deltaTime, EntityManager entityManager);
    }
}
