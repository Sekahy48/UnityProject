using Core.Contexts;
using ECS.Entity;
using ECS.Systems;
using MVC.Model;
using MVC.Presenter;
using MVC.View;
using Strategy;

namespace MVC.Controller
{
    /// <summary>
    /// Orquestador que agrupa los 3 contextos temáticos.
    /// Las clases NO deben recibir GameContext entero — deben recibir
    /// solo el sub-contexto que necesitan por constructor.
    /// GameMain usa este objeto para construir todo y luego inyectar
    /// los sub-contextos individuales donde hagan falta.
    /// </summary>
    public class GameContext
    {
        // ---- Sub-contextos Core ----
        public GameDataContext Data { get; private set; }
        public GameSessionContext Session { get; private set; }
        public GameSystemContext System { get; private set; }

        // ---- Piezas Unity (no van a Core) ----
        public CameraRegister CameraRegister { get; } = new();
        public InputManager InputManager { get; private set; }
        public HUDManager HUDManager { get; private set; }
        public ViewManager ViewManager { get; } = new();

        // ---- Builders ----

        public GameContext SetData(GameDataContext data)
        {
            Data = data;
            return this;
        }

        public GameContext SetSession(GameSessionContext session)
        {
            Session = session;
            return this;
        }

        public GameContext SetSystem(GameSystemContext system)
        {
            System = system;
            return this;
        }

        public GameContext SetInputManager(InputManager inputManager)
        {
            InputManager = inputManager;
            return this;
        }

        public GameContext SetHUDManager(HUDManager hudManager)
        {
            HUDManager = hudManager;
            return this;
        }
    }
}
