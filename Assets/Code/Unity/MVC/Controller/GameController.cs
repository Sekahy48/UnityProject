using Core.Contexts;
using ECS.Systems;
using MVC.View;

namespace MVC.Controller
{
    /// <summary>
    /// Main game controller.
    /// Receives only the sub-contexts it needs, not the whole GameContext.
    /// </summary>
    public class GameController
    {
        private readonly GameSystemContext systemCtx;
        private readonly InputManager inputManager;
        private readonly HUDManager hudManager;

        public GameController(GameSystemContext systemCtx, InputManager inputManager, HUDManager hudManager)
        {
            this.systemCtx = systemCtx;
            this.inputManager = inputManager;
            this.hudManager = hudManager;
        }

        /// <summary>
        /// Connects observers on startup. Example: HUD observes FatigueStaminaSystem.
        /// </summary>
        public void SetUpOnStart()
        {
            FatigueStaminaSystem staminaSystem = systemCtx.SystemManager
                .GetPeriodicSystem<FatigueStaminaSystem>();
            if (staminaSystem != null && hudManager != null)
            {
                staminaSystem.Attach(hudManager);
            }
        }

        /// <summary>
        /// Main game loop.
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. Input and camera (real time)
            inputManager.Update(deltaTime);

            // 2. SystemManager: engine systems (every frame) + game systems (per tick)
            systemCtx.SystemManager.Update(deltaTime);
        }
    }
}
