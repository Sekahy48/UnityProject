using Core.Contexts;
using Core.ECS.Systems;
using Core.MVC.View;
using MVC.View;

namespace MVC.Controller
{
    /// <summary>
    /// Main game controller.
    /// Receives only the sub-contexts it needs, not the whole GameContext.
    /// </summary>
    public class GameController
    {
        private readonly GameSystemContext _systemCtx;
        private readonly InputManager _inputManager;
        private readonly HUDManager _hudManager;

        public GameController(GameSystemContext systemCtx, InputManager inputManager, HUDManager hudManager)
        {
            _systemCtx = systemCtx;
            _inputManager = inputManager;
            _hudManager = hudManager;
        }

        /// <summary>
        /// Connects observers on startup. Example: HUD observes FatigueStaminaSystem.
        /// </summary>
        public void SetUpOnStart()
        {
            FatigueStaminaSystem staminaSystem = _systemCtx.SystemManager
                .GetPeriodicSystem<FatigueStaminaSystem>();
            if (staminaSystem != null && _hudManager != null)
            {
                staminaSystem.Attach(_hudManager);
            }
        }

        /// <summary>
        /// Main game loop.
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. Input and camera (real time)
            _inputManager.Update(deltaTime);

            // 2. SystemManager: engine systems (every frame) + game systems (per tick)
            _systemCtx.SystemManager.Update(deltaTime);
        }
    }
}
