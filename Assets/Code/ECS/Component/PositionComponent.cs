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
        private Vector3 rotation;

        private float xRotation;

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
        public override IComponent Clone()
        {
            return new PositionComponent(null);
        }

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
        
        public Vector3 GetRotation()
        {
            return this.rotation;
        }

        public void SetRotation(Vector3 rotation)
        {
            this.rotation = rotation;
        }

        public void ModifyRotation(Vector3 deltaRotation)
        {
            Quaternion delta = Quaternion.Euler(deltaRotation);
            _transform.rotation *= delta; 
        }


        public Quaternion GetQuaternionRotation(Quaternion rotation)
        {
            return Quaternion.Euler(this.rotation);
        }   

        public float GetXRotation()
        {
            return this.xRotation;
        }

        public void SetXRotation(float xRotation)
        {
            this.xRotation = xRotation;
        }

        public void ModifyXRotation(float deltaXRotation, Camera camera)
        {
            this.xRotation += deltaXRotation;
            this.xRotation = Mathf.Clamp(this.xRotation, -90f, 90f);

            camera.transform.localRotation = Quaternion.Euler(this.xRotation, 0f, 0f);
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
    }
}
