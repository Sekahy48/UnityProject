using ECS.Entity;
using ECS.Systems;

namespace Core.Contexts
{
    /// <summary>
    /// Session context: state of the current game session.
    /// Equivalent to SessionContext in StackGo.
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
