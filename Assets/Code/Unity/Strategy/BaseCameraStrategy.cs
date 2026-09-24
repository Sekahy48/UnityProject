using System;
using Core.ECS.Component;
using Core.ECS.Entity;
using MVC.View.Inventory;
using Core.Observer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    /// <summary>
    /// Base de las camaras con avatar (FPS y TPS). La RTS no hereda de aqui, y por eso esta
    /// clase es el sitio de lo que solo tienen las camaras con cuerpo: moverse, el
    /// inventario y la interaccion con el mundo.
    /// </summary>
    public abstract class BaseCameraStrategy : ICameraStrategy, IObserver, IInventoryInputSource,
                                               IWorldInteractionInputSource
    {
        protected Camera Camera;
        protected GameObject PlayerObject;
        protected IEntity player;
        protected Animator animator;

        protected Vector2 rotation = Vector2.zero;

        public event Action OnInventoryToggleRequested;
        public event Action OnInventoryCancelRequested; 
        public event Action<PanelType> OnInventoryPanelToggleRequested;

        public event Action OnInteractTapped;
        public event Action OnInteractHoldStarted;
        public event Action OnInteractHoldReleased;

        /// <summary>
        /// Segundos que hay que mantener la tecla para que deje de ser un toque. El toque se
        /// ejecuta al SOLTAR, no al pulsar: si recogiera al pulsar, al llegar al umbral ya se
        /// habria recogido. El precio es hasta este retraso en el toque. Constante de ajuste:
        /// se afina jugando.
        /// </summary>
        private const float INTERACT_HOLD_TIME = 0.25f;

        private bool _interactPressed;
        private bool _interactHoldFired;
        private float _interactHeldFor;

        protected BaseCameraStrategy(IEntity player, string cameraName)
        {
            this.player = player;
            this.PlayerObject = GameObject.FindWithTag("MainPlayer");
            this.Camera = new GameObject(cameraName).AddComponent<Camera>();
            this.animator = PlayerObject.GetComponent<Animator>();
        }

        // Common methods
        public virtual void Activate() => Camera.enabled = true;
        /// <summary>
        /// Al dejar de ser la camara activa se olvida una pulsacion a medias: si no, volver a
        /// esta camara con la E ya soltada dejaria un "mantener" colgado.
        /// </summary>
        public virtual void Deactivate()
        {
            Camera.enabled = false;
            _interactPressed = false;
            _interactHoldFired = false;
        }

        protected internal MovementComponent GetMov() => player.GetComponent<MovementComponent>();
         

        public void Execute(float deltaTime)
        {
            HandleMouseLook(deltaTime);
            WriteGaze();
            HandleMovement(deltaTime);
            HandleInventoryInput();
            HandleWorldInteractionInput(deltaTime);
        }

        public abstract void Update();

        protected virtual void HandleAnimation(float horizontal, float vertical, float deltaTime)
        {
            if (animator == null)
                return;

            var mov = GetMov();
            animator.SetBool("IsRunning", mov.IsRunning);
            animator.SetBool("IsJumping", mov.IsJumping);

            float smoothHorizontal = Mathf.Lerp(animator.GetFloat("VelX"), horizontal, deltaTime * 10f);
            float smoothVertical = Mathf.Lerp(animator.GetFloat("VelY"), vertical, deltaTime * 10f);

            animator.SetFloat("VelX", smoothHorizontal);
            animator.SetFloat("VelY", smoothVertical);
        }

        protected void HandleInventoryInput()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame) 
                OnInventoryToggleRequested?.Invoke(); 
            else if (Keyboard.current.escapeKey.wasPressedThisFrame) 
                OnInventoryCancelRequested?.Invoke(); 
            else if (Keyboard.current.shiftKey.isPressed)
            {
                PanelType panel;
                if (Keyboard.current.digit1Key.wasPressedThisFrame) 
                    panel = PanelType.A;
                else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                    panel = PanelType.B;
                else return;

                Debug.Log(panel.ToString());
                OnInventoryPanelToggleRequested?.Invoke(panel);
            }
                
        }

        /// <summary>
        /// Pasa a Core hacia donde mira el jugador: la direccion de la camara, con su
        /// inclinacion. En TPS tambien la de la camara y no la del personaje, porque lo que
        /// el jugador espera es interactuar con lo que tiene en el centro de la pantalla; el
        /// origen, en cambio, lo pone Core en los ojos del personaje, no en la camara, para
        /// que la camara de detras no alcance cosas que el personaje tiene a la espalda.
        /// </summary>
        private void WriteGaze()
        {
            Vector3 forward = Camera.transform.forward;
            player.GetComponent<GazeComponent>()?.SetDirection(forward.x, forward.y, forward.z);
        }

        /// <summary>
        /// Lee la E y la convierte en toque, inicio de mantener o fin de mantener. No sabe
        /// que hay delante ni que se hara: eso es del presentador.
        /// </summary>
        protected void HandleWorldInteractionInput(float deltaTime)
        {
            var key = Keyboard.current.eKey;

            if (key.wasPressedThisFrame)
            {
                _interactPressed = true;
                _interactHoldFired = false;
                _interactHeldFor = 0f;
            }
            else if (_interactPressed && key.isPressed)
            {
                _interactHeldFor += deltaTime;
                if (!_interactHoldFired && _interactHeldFor >= INTERACT_HOLD_TIME)
                {
                    _interactHoldFired = true;
                    OnInteractHoldStarted?.Invoke();
                }
            }

            if (_interactPressed && key.wasReleasedThisFrame)
            {
                _interactPressed = false;
                if (_interactHoldFired) OnInteractHoldReleased?.Invoke();
                else OnInteractTapped?.Invoke();
            }
        }

        public Camera GetCamera() => Camera;

        protected abstract void HandleMouseLook(float deltaTime);

        /// <summary>
        /// Unified FPS/TPS movement. Works directly with Transform
        /// (not with PositionComponent). TransformSyncSystem syncs it to Core.
        /// </summary>
        protected virtual void HandleMovement(float deltaTime)
        {
            Transform tr = PlayerObject.transform;
            Vector3 move = Vector3.zero;
            var movComp = GetMov();

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.wKey.isPressed) { move += tr.forward; vertical += 1f; }
            if (Keyboard.current.sKey.isPressed) { move -= tr.forward; vertical -= 1f; }
            if (Keyboard.current.aKey.isPressed) { move -= tr.right; horizontal -= 1f; }
            if (Keyboard.current.dKey.isPressed) { move += tr.right; horizontal += 1f; }

            HandleAnimation(horizontal, vertical, deltaTime);

            if (move != Vector3.zero)
            {
                move.Normalize();
                movComp.SetIsJumping(Keyboard.current.spaceKey.isPressed);
                movComp.SetIsRunning(Keyboard.current.leftShiftKey.isPressed);
                float speed = movComp.IsRunning && movComp.CanRun()
                    ? movComp.GetSpeed() * movComp.RunMultiplier
                    : movComp.GetSpeed();

                tr.position += move * speed * deltaTime;
            }
            else
            {
                movComp.SetIsRunning(false);
                movComp.SetIsJumping(false);
            }
        }
    }
}
