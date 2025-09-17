using UnityEngine;

namespace ECS.Component
{
    public class MovementComponent : BasicComponent
    {
        private float Speed;
        private Vector2 Direction;
        private bool _isMoving;
        public MovementComponent(float speed)
        {
            Speed = speed;
            Direction = Vector2.zero; // Inicializa la dirección a un vector nulo
            this._isMoving = false; // Inicializa el estado de movimiento a falso
        }

        public void SetSpeed(float speed)
        {
            Speed = speed;
        }

        public float GetSpeed()
        {
            return Speed;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 GetDirection()
        {
            return Direction;
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
            throw new System.NotImplementedException();
        }
    }
}