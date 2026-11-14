using Core.ECS.Systems;
using Core.MVC.Presenter;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Contexts
{
    /// <summary>
    /// Infrastructure context: game systems, presenters.
    /// Equivalent to SystemContext in StackGo.
    /// </summary>
    public class GameSystemContext
    {
        public SystemManager SystemManager { get; }
        public PresenterManager PresenterManager { get; }

        public GameSystemContext(SystemManager systemManager, PresenterManager presenterManager)
        {
            AC.CheckNotNull(systemManager, nameof(systemManager));
            AC.CheckNotNull(presenterManager, nameof(presenterManager));
            SystemManager = systemManager;
            PresenterManager = presenterManager;
        }
    }
}
