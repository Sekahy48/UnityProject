using ECS.Entity;
using ECS.Systems;

namespace Core.Contexts
{
    /// <summary>
    /// Contexto de sesión: estado de la partida en curso.
    /// Equivalente a SessionContext en StackGo.
    /// </summary>
    public class GameSessionContext
    {
        public IEntity Player { get; private set; }
        public ClockSystem Clock => ClockSystem.GetInstance();

        public void SetPlayer(IEntity player)
        {
            Player = player;
        }
    }
}
