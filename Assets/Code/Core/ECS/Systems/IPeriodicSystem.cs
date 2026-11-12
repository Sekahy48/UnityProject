using System;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Interfaz de los sistemas del bucle de juego.
    /// Cada sistema procesa las entidades que tienen los componentes que le interesan.
    /// </summary>
    public interface IPeriodicSystem
    {
        /// <summary>
        /// Procesa un tick. El sistema pide a EntityManager las entidades que le interesan.
        /// </summary>
        /// <param name="deltaTime">Duracion del tick (el tickTime de ClockSystem para los sistemas de juego, el deltaTime real para los del motor)</param>
        /// <param name="entityManager">Acceso a las entidades del juego</param>
        void Process(float deltaTime, EntityManager entityManager);
    }
}
