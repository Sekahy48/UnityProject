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

        public GameController(GameSystemContext systemCtx, InputManager inputManager)
        {
            _systemCtx = systemCtx;
            _inputManager = inputManager;
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
