using ECS.Systems;
using MVC.Model;
using MVC.View;
using Strategy;

namespace MVC.Controller
{
    public class GameController
    {
        private GameContext GameContext;
        private GameMain GameMain;

        public GameController(GameContext gameContext, GameMain gameMain)
        {
            GameContext = gameContext;
            GameMain = gameMain;
        }

        public GameController() { }

        public void SetUpOnStart()
        {
            // HUD observa el sistema de stamina/fatiga para actualizar barras
            FatigueStaminaSystem staminaSystem = GameContext.GetSystemManager()
                .GetGameSystem<FatigueStaminaSystem>();
            if (staminaSystem != null)
            {
                HUDManager hUDManager = GameContext.GetHUDManager();
                staminaSystem.Attach(hUDManager);
            }
        }

        public void SetGameContext(GameContext gameContext)
        {
            GameContext = gameContext;
        }

        public GameContext GetGameContext()
        {
            return GameContext;
        }

        public void SetGameMain(GameMain gameMain)
        {
            GameMain = gameMain;
        }

        public GameMain GetGameMain()
        {
            return GameMain;
        }

        /// <summary>
        /// Ciclo principal del juego.
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. Input y cámara (tiempo real, no afectado por pausa/timeSpeed)
            GameContext.GetInputManager().Update(deltaTime);

            // 2. SystemManager: engine systems (cada frame) + game systems (por tick de ClockSystem)
            GameContext.GetSystemManager().Update(deltaTime);
        }
    }
}
