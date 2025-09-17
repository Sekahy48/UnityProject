using MVC.Model;
using Strategy;

namespace MVC.Controller
{
    public class GameController
    {
        private GameContext GameContext;
        private GameMain GameMain;
        private readonly ICameraStrategy activeCamera;

        public GameController(GameContext gameContext, GameMain gameMain)
        {
            GameContext = gameContext;
            GameMain = gameMain;
        }
        public GameController()
        {
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
        /// Ciclo de gestion del GameController
        /// </summary>
        public void Update(float deltaTime)
        {
            // Lógica del juego
            //GameContext.GetLogic().Update();

            // Actualizar cámara activa
            GameContext.GetCameraRegister().GetActiveCamera().Execute(deltaTime);
        }
    }

}