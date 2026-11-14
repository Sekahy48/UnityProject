using Core.Contexts;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.MVC.Model;
using Core.MVC.Presenter;
using Core.MVC.View;
using MVC.View;
using Strategy;

namespace MVC.Controller
{
    /// <summary>
    /// Orquestador que agrupa los 3 contextos tematicos.
    /// Las clases NO deben recibir el GameContext entero: reciben por constructor
    /// solo el subcontexto que necesitan.
    /// GameMain usa este objeto para montarlo todo y despues inyecta
    /// cada subcontexto donde haga falta.
    /// </summary>
    public class    GameContext
    {
        // ---- Subcontextos del Core ----
        public GameDataContext Data { get; private set; }
        public GameSessionContext Session { get; private set; }
        public GameSystemContext System { get; private set; }
        public GameInteractionContext Interaction { get; private set; }

        // ---- Piezas de Unity (no van en el Core) ----
        public InputManager InputManager { get; private set; }

        // ---- Constructores ----

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

        public GameContext SetInteraction(GameInteractionContext interaction)
        {
            Interaction = interaction;
            return this;
        }

        public GameContext SetInputManager(InputManager inputManager)
        {
            InputManager = inputManager;
            return this;
        }
    }
}
