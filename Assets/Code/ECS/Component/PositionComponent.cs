using UnityEngine;
using ECS.Component;
using System;

namespace ECS.Component
{
    /// <summary>
    /// Componente para almacenar la posición de una entidad
    /// </summary>
    public class PositionComponent : BasicComponent, IComponent
    {
        
        private Transform _transform;
        /*
        private float x;
        private float y; // Altura
        private float z;
    */ 
        public PositionComponent(Transform transform)
        {
            this._transform = transform;
        }
    /*
            public PositionComponent(float x, float y, float z, Transform transform)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this._transform = transform;
                this.name = "PositionComponent";
            } 
    */

        /*
        // Getters
        public float X => x;
        public float Y => y;
        public float Z => z;

        public Vector3 Coordinates => new Vector3(x, y, z);

        // Setters
        public void SetX(float value) => x = value;
        public void SetY(float value) => y = value;
        public void SetZ(float value) => z = value;

        public void SetCoordinates(Vector3 coordinates)
        {
            x = coordinates.x;
            y = coordinates.y;
            z = coordinates.z;
        }
        */
        public void SetTransform(Transform transform)
        {
            this._transform = transform; 
        }

        public Transform GetTransform()
        {
            return this._transform == null ? throw new NullReferenceException() : this._transform;
        }
         
        /*
        // Métodos incrementadores
        public void IncrementX(float deltaX) => x += deltaX;
        public void IncrementY(float deltaY) => y += deltaY;
        public void IncrementZ(float deltaZ) => z += deltaZ;

        public void IncrementPosition(float deltaX, float deltaY, float deltaZ)
        {
            x += deltaX;
            y += deltaY;
            z += deltaZ;
        }

        public void IncrementPosition(Vector3 delta)
        {
            x += delta.x;
            y += delta.y;
            z += delta.z;
        }
        */
        public Vector3 Forward()
        {
            return this._transform.forward;
        }

        public Vector3 Right()
        {
            return this._transform.right;
        }



        public Transform ModifyPosition(Vector3 deltaPosition, float speed, float deltaTime)
        {
            this._transform.position += deltaPosition * speed * deltaTime;
            return this._transform;
        }
        

        public override string ToString()
        {
            return $"PositionComponent{_transform.position.ToString()}";
        }

        public override IComponent Clone()
        {
            return new PositionComponent(null);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is PositionComponent otherPosition &&
                this._transform.position == otherPosition._transform.position &&
                this._transform.rotation == otherPosition._transform.rotation &&
                this._transform.localScale == otherPosition._transform.localScale;
        }
    }
}
