using ECS.Systems;
using MVC.Presenter;

namespace Core.Contexts
{
    /// <summary>
    /// Contexto de infraestructura: sistemas de juego, presenters.
    /// Equivalente a SystemContext en StackGo.
    /// </summary>
    public class GameSystemContext
    {
        public SystemManager SystemManager { get; }
        public PresenterManager PresenterManager { get; }

        public GameSystemContext(SystemManager systemManager, PresenterManager presenterManager)
        {
            SystemManager = systemManager;
            PresenterManager = presenterManager;
        }
    }
}
