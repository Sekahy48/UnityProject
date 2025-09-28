using Observer;
using UnityEngine;

namespace ECS.Component
{
    public class MovementComponent : BasicComponent
    {
        private float Speed;
        private float RunMultiplier = 1.5f;
        private Vector2 Direction;
        private bool _isMoving;
        private float MouseSensitivity = 1f;
        public MovementComponent(float speed)
        {
            Speed = speed;
            Direction = Vector2.zero; // Inicializa la dirección a un vector nulo
            this._isMoving = false; // Inicializa el estado de movimiento a falso
        }

        public MovementComponent(float speed, float mult, Vector2 direction, bool isMoving, float mouseSensitivity)
        {
            this.Speed = speed;
            this.RunMultiplier = mult;
            this.Direction = direction;
            this._isMoving = isMoving; // Inicializa el estado de movimiento a falso
            this.MouseSensitivity = mouseSensitivity;
        }
        

        public void SetSpeed(float speed)
        {
            Speed = speed;
        }

        public float GetSpeed()
        {
            return Speed;
        }

        public void SetRunMultiplier(float mult)
        {
            RunMultiplier = mult;
        }
        public float GetRunMultiplier()
        {
            return RunMultiplier;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 GetDirection()
        {
            return Direction;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            MouseSensitivity = sensitivity;
        }

        public float GetMouseSensitivity()
        {
            return MouseSensitivity;
        }
        
        public void switchIsMoving()
        {
            _isMoving = !_isMoving;
        }
        
        public bool IsMoving()
        {
            return _isMoving;
        }

        public override IComponent Clone()
        {
            return new MovementComponent(this.Speed, this.RunMultiplier, this.Direction, this._isMoving, this.MouseSensitivity);
        }
    }
}