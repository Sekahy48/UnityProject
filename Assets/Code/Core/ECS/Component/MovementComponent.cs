namespace ECS.Component
{
    /// <summary>
    /// Componente de movimiento puro C#. Sin dependencias de UnityEngine.
    /// Las cámaras (Unity) leen/escriben estos valores; los sistemas Core los procesan.
    /// </summary>
    public class MovementComponent : BasicComponent
    {
        private float _speed;
        private float _runMultiplier = 2.5f;
        private float _dirX, _dirY;
        private bool _isMoving;
        private bool _isRunning = false;
        private bool _canRun;
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
        public float GetSpeed() => _speed;

        // ---- Run multiplier ----

        public void SetRunMultiplier(float mult) => _runMultiplier = mult;
        public float GetRunMultiplier() => _runMultiplier;

        // ---- Direction ----

        public void SetDirection(float x, float y) { _dirX = x; _dirY = y; }
        public float GetDirX() => _dirX;
        public float GetDirY() => _dirY;

        // ---- Mouse sensitivity ----

        public void SetMouseSensitivity(float sensitivity) => _mouseSensitivity = sensitivity;
        public float GetMouseSensitivity() => _mouseSensitivity;

        // ---- Movement state ----

        public void SwitchIsMoving() => _isMoving = !_isMoving;
        public bool IsMoving() => _isMoving;

        public void SetIsRunning(bool running)
        {
            if (!CanRun())
                _isRunning = false;
            else
                _isRunning = running;
        }

        public bool IsRunning() => _isRunning;

        public void SetIsJumping(bool jump) => _wantsToJump = jump;
        public bool IsJumping() => _wantsToJump;

        public void SetCanRun(bool canRun)
        {
            SetIsRunning(false);
            _canRun = canRun;
        }

        public bool CanRun() => _canRun;

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
