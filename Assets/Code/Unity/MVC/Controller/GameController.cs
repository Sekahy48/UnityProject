using Core.Contexts;
using ECS.Systems;
using MVC.View;

namespace MVC.Controller
{
    /// <summary>
    /// Controlador principal del juego.
    /// Recibe solo los sub-contextos que necesita, no el GameContext entero.
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
        /// Conecta observers al arrancar. Ejemplo: HUD observa FatigueStaminaSystem.
        /// </summary>
        public void SetUpOnStart()
        {
            FatigueStaminaSystem staminaSystem = systemCtx.SystemManager
                .GetGameSystem<FatigueStaminaSystem>();
            if (staminaSystem != null && hudManager != null)
            {
                staminaSystem.Attach(hudManager);
            }
        }

        /// <summary>
        /// Ciclo principal del juego.
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. Input y cámara (tiempo real)
            inputManager.Update(deltaTime);

            // 2. SystemManager: engine systems (cada frame) + game systems (por tick)
            systemCtx.SystemManager.Update(deltaTime);
        }
    }
}
