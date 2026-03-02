using Observer;
using UnityEngine;

namespace ECS.Component
{
    public class MovementComponent : BasicComponent
    {
        private float _speed;
        private float _runMultiplier = 2.5f;
        private Vector2 _direction;
        private bool _isMoving;
        private bool _isRunning = false;
        private bool _canRun;
        private bool _wantsToJump = false;
        private float _mouseSensitivity = 1f;

        public MovementComponent(float speed)
        {
            _speed = speed;
            _direction = Vector2.zero; // Inicializa la dirección a un vector nulo
            this._isMoving = false; // Inicializa el estado de movimiento a falso
        }

        public MovementComponent(float speed, float mult, Vector2 direction, bool isMoving, float mouseSensitivity)
        {
            this._speed = speed;
            this._runMultiplier = mult;
            this._direction = direction;
            this._isMoving = isMoving; // Inicializa el estado de movimiento a falso
            this._mouseSensitivity = mouseSensitivity;
        }
        

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public void SetRunMultiplier(float mult)
        {
            _runMultiplier = mult;
        }
        public float GetRunMultiplier()
        {
            return _runMultiplier;
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;
        }

        public Vector2 GetDirection()
        {
            return _direction;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            _mouseSensitivity = sensitivity;
        }

        public float GetMouseSensitivity()
        {
            return _mouseSensitivity;
        }
        
        public void switchIsMoving()
        {
            _isMoving = !_isMoving;
        }

        public bool IsMoving()
        {
            return _isMoving;
        }

        public void SetIsRunning(bool running)
        {
            if(!CanRun())
                _isRunning = false;
            else
            _isRunning = running;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public void SetIsJumping(bool jump)
        {
            _wantsToJump = jump;
        }

        public bool IsJumping()
        {
            return _wantsToJump;
        }

        public void SetCanRun(bool canRun)
        {
            this.SetIsRunning(false);
            _canRun = canRun;
            
        }
        
        public bool CanRun()
        {
            return _canRun;
        }

        public override IComponent Clone()
        {
            return new MovementComponent(this._speed, this._runMultiplier, this._direction, this._isMoving, this._mouseSensitivity);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is MovementComponent otherMovement &&
                this._speed == otherMovement._speed &&
                this._runMultiplier == otherMovement._runMultiplier &&
                this._direction == otherMovement._direction &&
                this._isMoving == otherMovement._isMoving &&
                this._mouseSensitivity == otherMovement._mouseSensitivity;
        }
    }
}