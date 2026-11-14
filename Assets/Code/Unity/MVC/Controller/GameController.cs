using Core.Contexts;
using Core.ECS.Systems;
using Core.MVC.View;
using MVC.View;

namespace MVC.Controller
{
    /// <summary>
    /// Controlador principal del juego.
    /// Recibe solo los subcontextos que necesita, no el GameContext entero.
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
        /// Bucle principal del juego.
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. Entrada y camara (tiempo real)
            _inputManager.Update(deltaTime);

            // 2. SystemManager: sistemas del motor (cada fotograma) + sistemas de juego (por tick)
            _systemCtx.SystemManager.Update(deltaTime);
        }
    }
}
