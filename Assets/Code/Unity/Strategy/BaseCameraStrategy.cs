using System;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.MVC.View.UI.Inventory;
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
        protected Camera Camera { get; }
        protected GameObject PlayerObject { get; }
        protected Rigidbody PlayerBody { get; }
        protected IEntity Player { get; }
        protected Animator Animator { get; }

        /* Campo y no propiedad a proposito: es un struct que las subclases modifican por partes
           (Rotation.x += ...), y eso no compila sobre una propiedad. */
        protected Vector2 Rotation = Vector2.zero;

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

        /// <summary>
        /// Quien decide si la camara puede girar con el raton. Lo pone InputManager al
        /// activar la estrategia; sin el, la camara gira siempre, que es lo que hacia antes.
        /// </summary>
        public Unity.LookControl Look { get; set; }

        private bool _interactPressed;
        private bool _interactHoldFired;
        private float _interactHeldFor;
        
        // Materiales de fisicas
        private readonly CapsuleCollider _playerCollider;
        private readonly PhysicsMaterial _movingMaterial;   // PlayerSinFriccion (el del Inspector)
        private readonly PhysicsMaterial _idleMaterial;     // agarre total, creado aqui

        protected BaseCameraStrategy(IEntity player, string cameraName)
        {
            Player = player;
            PlayerObject = GameObject.FindWithTag("MainPlayer");
            Rigidbody rigidbody = PlayerObject.GetComponent<Rigidbody>();
            if (rigidbody == null)  
                throw new InvalidOperationException("No se puede inicializar la estrategia de camara: el jugador no tiene Rigidbody en Unity.");
            PlayerBody = rigidbody;

            _playerCollider = PlayerObject.GetComponent<CapsuleCollider>();
            _movingMaterial = _playerCollider.sharedMaterial;
            _idleMaterial = new PhysicsMaterial("PlayerQuieto")
            {
                staticFriction = 1f,
                dynamicFriction = 1f,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };

            Camera = new GameObject(cameraName).AddComponent<Camera>();
            Animator = PlayerObject.GetComponent<Animator>();
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
            PlayerBody.linearVelocity = new Vector3(0f, PlayerBody.linearVelocity.y, 0f);
            HandleAnimation(0f, 0f, 1f);   
        }

        protected internal MovementComponent GetMov() => Player.GetComponent<MovementComponent>();
         

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
            if (Animator == null)
                return;

            var mov = GetMov();
            Animator.SetBool("IsRunning", mov.IsRunning);
            Animator.SetBool("IsJumping", mov.IsJumping);

            float smoothHorizontal = Mathf.Lerp(Animator.GetFloat("VelX"), horizontal, deltaTime * 10f);
            float smoothVertical = Mathf.Lerp(Animator.GetFloat("VelY"), vertical, deltaTime * 10f);

            Animator.SetFloat("VelX", smoothHorizontal);
            Animator.SetFloat("VelY", smoothVertical);
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
        /// <summary>
        /// Lo que se ha movido el raton para girar la vista: cero con la vista bloqueada.
        ///
        /// <para>Se bloquea la ENTRADA y no el metodo entero. <c>HandleMouseLook</c> tambien
        /// coloca la camara (en TPS la lleva detras del personaje); saltarlo dejaba la camara
        /// clavada donde estaba mientras el personaje se alejaba. Asi la camara sigue al
        /// personaje y solo deja de girar.</para>
        /// </summary>
        protected Vector2 ReadLookDelta()
        {
            if (Look != null && Look.IsLocked) return Vector2.zero;
            return Mouse.current.delta.ReadValue();
        }

        private void WriteGaze()
        {
            Vector3 forward = Camera.transform.forward;
            Player.GetComponent<GazeComponent>()?.SetDirection(forward.x, forward.y, forward.z);
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
        /// Movimiento comun de FPS y TPS, por fisica: no escribe la posicion, le da velocidad
        /// al Rigidbody del jugador y el motor fisico lo mueve y resuelve los choques.
        /// TransformSyncSystem copia despues la posicion al Core.
        ///
        /// <para>La velocidad horizontal real se acerca cada fotograma a la objetivo
        /// (<see cref="MovementComponent.GetSpeed"/> en la direccion de las teclas, o cero sin
        /// teclas) al ritmo de <see cref="MovementComponent.Acceleration"/> o, al frenar,
        /// <see cref="MovementComponent.Deceleration"/>. Se parte de la velocidad REAL del cuerpo:
        /// si un choque la ha cortado, vuelve a acelerar desde ahi. La vertical no se toca, para
        /// no anular la gravedad.</para>
        /// </summary>
        protected virtual void HandleMovement(float deltaTime)
        {
            Transform tr = PlayerObject.transform;
            Vector3 move = Vector3.zero;
            var movComp = GetMov();

            float horizontal = 0f;
            float vertical = 0f;

            Quaternion yaw = Quaternion.Euler(0f, Rotation.x, 0f);
            Vector3 forward = yaw * Vector3.forward;
            Vector3 right = yaw * Vector3.right;

            if (Keyboard.current.wKey.isPressed) { move += forward; vertical += 1f; }
            if (Keyboard.current.sKey.isPressed) { move -= forward; vertical -= 1f; }
            if (Keyboard.current.aKey.isPressed) { move -= right; horizontal -= 1f; }
            if (Keyboard.current.dKey.isPressed) { move += right; horizontal += 1f; }

            HandleAnimation(horizontal, vertical, deltaTime);
            Vector3 targetVelocity = Vector3.zero;
            if (move != Vector3.zero)
            {
                move.Normalize();
                movComp.SetIsJumping(Keyboard.current.spaceKey.isPressed);
                movComp.SetIsRunning(Keyboard.current.leftShiftKey.isPressed);
                targetVelocity = move * movComp.GetSpeed(); 
            }
            else
            {
                movComp.SetIsRunning(false);
                movComp.SetIsJumping(false);
            }
        

            Vector3 current = Vector3.ProjectOnPlane(PlayerBody.linearVelocity, Vector3.up);
            float currentSpeed = current.magnitude;
            float targetSpeed = targetVelocity.magnitude;

            // Rapidez: sube con Acceleration, baja con Deceleration (soltar teclas o dejar de correr)
            float rate = targetSpeed > currentSpeed ? movComp.Acceleration : movComp.Deceleration;
            float nextSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * deltaTime);

            // Direccion: gira hacia la de las teclas sin gastar rapidez
            Vector3 nextDir;
            if (targetSpeed == 0f)
                nextDir = currentSpeed > 0.01f ? current / currentSpeed : Vector3.zero; // frenando: mantiene direccion
            else if (currentSpeed < 0.01f)
                nextDir = move;                                                          // parado: arranca ya hacia donde pide
            else
                nextDir = Vector3.RotateTowards(current / currentSpeed, move,
                                                movComp.TurnSpeed * Mathf.Deg2Rad * deltaTime, 0f);

            Vector3 next = nextDir * nextSpeed;
            _playerCollider.sharedMaterial = targetSpeed == 0f ? _idleMaterial : _movingMaterial;
            PlayerBody.linearVelocity = new Vector3(next.x, PlayerBody.linearVelocity.y, next.z);
        }
    }
}
