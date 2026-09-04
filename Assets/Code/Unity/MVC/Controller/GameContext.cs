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
    /// Orchestrator that groups the 3 thematic contexts.
    /// Classes should NOT receive the whole GameContext — they should
    /// receive only the sub-context they need via constructor.
    /// GameMain uses this object to build everything and then inject
    /// the individual sub-contexts wherever needed.
    /// </summary>
    public class    GameContext
    {
        // ---- Core sub-contexts ----
        public GameDataContext Data { get; private set; }
        public GameSessionContext Session { get; private set; }
        public GameSystemContext System { get; private set; }
        public GameInteractionContext Interaction { get; private set; }

        // ---- Unity pieces (do not go in Core) ---- 
        public InputManager InputManager { get; private set; }
        public HUDManager HUDManager { get; private set; } 

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

        public GameContext SetHUDManager(HUDManager hudManager)
        {
            HUDManager = hudManager;
            return this;
        }
    }
}
