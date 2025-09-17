using UnityEngine;
using ECS.Component;

namespace ECS.Component
{
    /// <summary>
    /// Componente para almacenar la posición de una entidad
    /// </summary>
    public class PositionComponent : BasicComponent, IComponent
    {
        private float x;
        private float y; // Altura
        private float z;

        public PositionComponent(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.name = "PositionComponent";
        }

        public override IComponent Clone()
        {
            return new PositionComponent(x, y, z);
        }

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

        public override string ToString()
        {
            return $"PositionComponent{{x={x}, y={y}, z={z}}}";
        }
    }
}
