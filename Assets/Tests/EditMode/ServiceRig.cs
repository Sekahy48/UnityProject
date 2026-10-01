using Core.Contexts;
using Core.ECS.Systems;
using Core.Events;
using Core.MVC.Presenter;
using Core.Services;

namespace Core.Tests
{
    /// <summary>
    /// Monta lo minimo para usar InventoryService fuera del juego: un SystemManager con los
    /// sistemas de inventario y equipo, una mano vacia y un logger mudo.
    ///
    /// <para>Cada test crea el suyo. Antes limpia el estado estatico que se arrastraria entre
    /// tests: los suscriptores del EventBus (los sistemas se suscriben al registrarse) y el
    /// logger, que el juego asigna desde Unity y aqui no existe.</para>
    /// </summary>
    internal sealed class ServiceRig
    {
        public readonly InventoryService Service;

        public ServiceRig()
        {
            EventBus.GetInstance().Clear();
            CoreLogger.Instance = new SilentLogger();

            // Sin EntityManager: InventoryService y estos dos sistemas no lo usan.
            SystemManager systems = new SystemManager(null)
                .RegisterReactiveGameSystem(new InventorySystem())
                .RegisterReactiveGameSystem(new EquipmentSystem());

            Service = new InventoryService(new GameInteractionContext(),
                                           new GameSystemContext(systems, new PresenterManager()));
        }

        private sealed class SilentLogger : ILogger
        {
            public void Log(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
        }
    }
}
