using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Pure C# movement component. No UnityEngine dependencies.
    /// Cameras (Unity) read/write these values; Core systems process them.
    /// </summary>
    public class MovementComponent : BasicComponent
    {
        private float _speed;
        public float TurnSpeed {get;} = 720;
        private float _runMultiplier = 2.5f;

        /* Ritmo al que la velocidad real se acerca a la objetivo, en m/s cada segundo.
           Frenar es mas rapido que arrancar: el cuerpo se para antes de lo que arranca.
           Valores de partida para ajustar jugando (con velocidad 2: ~0,25 s para arrancar). */
        private float _acceleration = 8f;
        private float _deceleration = 16f;
        private float _weightSpeedMultiplier = 1.0f;
        private float _dirX, _dirY;
        private bool _isMoving;
        private bool _isRunning = false;
        private int _runRestrictions = 0;
        private bool _wantsToJump = false;
        private float _mouseSensitivity = 1f;

        public MovementComponent(float speed)
        {
            _speed = speed;
            _dirX = 0f;
            _dirY = 0f;
            _isMoving = false; 
        }

        public MovementComponent(float speed, float mult, float dirX, float dirY, bool isMoving, float mouseSensitivity)
        {
            _speed = speed;
            _runMultiplier = mult;
            _dirX = dirX;
            _dirY = dirY;
            _isMoving = isMoving;
            _mouseSensitivity = mouseSensitivity;
        }

        // ---- Speed ----

        public void SetSpeed(float speed) => _speed = speed;
        /// <summary>
        /// Velocidad a la que se quiere ir ahora, en m/s: la base por el multiplicador de peso
        /// y, si esta corriendo y puede correr, por el de correr. Es el OBJETIVO al que tiende la
        /// aceleracion, no la velocidad real del cuerpo. Ya lo incluye todo: no multiplicar otra
        /// vez por <see cref="RunMultiplier"/> desde fuera.
        /// </summary>
        public float GetSpeed() => IsRunning && CanRun()
                    ? _speed * _weightSpeedMultiplier * RunMultiplier
                    : _speed * _weightSpeedMultiplier;
        public void SetWeightSpeedMultiplier(float mult) => _weightSpeedMultiplier = mult;
        public float WeightSpeedMultiplier => _weightSpeedMultiplier;

        // ---- Acceleration ----

        public float Acceleration => _acceleration;
        public void SetAcceleration(float acceleration) => _acceleration = acceleration;

        public float Deceleration => _deceleration;
        public void SetDeceleration(float deceleration) => _deceleration = deceleration;

        // ---- Run multiplier ----

        public void SetRunMultiplier(float mult) => _runMultiplier = mult;
        public float RunMultiplier => _runMultiplier;

        // ---- Direction ----

        public void SetDirection(float x, float y) { _dirX = x; _dirY = y; }
        public float DirX => _dirX;
        public float DirY => _dirY;

        // ---- Mouse sensitivity ----

        public void SetMouseSensitivity(float sensitivity) => _mouseSensitivity = sensitivity;
        public float MouseSensitivity => _mouseSensitivity;
 

        // ---- Movement state ----

        public void SwitchIsMoving() => _isMoving = !_isMoving;
        public bool IsMoving => _isMoving;

        public void SetIsRunning(bool running)
        {
            if (!CanRun())
                _isRunning = false;
            else
                _isRunning = running;
        }

        public bool IsRunning => _isRunning;

        public void SetIsJumping(bool jump) => _wantsToJump = jump;
        public bool IsJumping => _wantsToJump;

        public void AddRunRestriction() { _runRestrictions++; SetIsRunning(false); }
        public void RemoveRunRestriction() { _runRestrictions = Math.Max(0, _runRestrictions - 1); }
        public bool CanRun() => _runRestrictions == 0;

        // ---- IComponent ----

        public override IComponent Clone()
        {
            return new MovementComponent(_speed, _runMultiplier, _dirX, _dirY, _isMoving, _mouseSensitivity);
        }

        public override bool Equivalent(IComponent other)
        {
            return other is MovementComponent o &&
                   _speed == o._speed &&
                   _runMultiplier == o._runMultiplier &&
                   _dirX == o._dirX && _dirY == o._dirY &&
                   _isMoving == o._isMoving &&
                   _mouseSensitivity == o._mouseSensitivity;
        }
    }
}
