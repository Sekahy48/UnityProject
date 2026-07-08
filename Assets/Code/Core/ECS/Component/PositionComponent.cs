using System;

namespace ECS.Component
{
    /// <summary>
    /// Componente de posición puro C#. Sin dependencias de UnityEngine.
    /// TransformSyncSystem (Unity) se encarga de sincronizar estos valores
    /// con el Transform del GameObject cada frame.
    /// </summary>
    public class PositionComponent : BasicComponent, IComponent
    {
        private float _posX, _posY, _posZ;
        private float _rotX, _rotY, _rotZ, _rotW;
        private bool _dirty;

        public PositionComponent(float x, float y, float z)
        {
            _posX = x; _posY = y; _posZ = z;
            _rotX = 0f; _rotY = 0f; _rotZ = 0f; _rotW = 1f;
            _dirty = false;
        }

        public PositionComponent(float x, float y, float z, float rotX, float rotY, float rotZ, float rotW)
        {
            _posX = x; _posY = y; _posZ = z;
            _rotX = rotX; _rotY = rotY; _rotZ = rotZ; _rotW = rotW;
            _dirty = false;
        }

        // ---- Posición ----

        public float GetX() => _posX;
        public float GetY() => _posY;
        public float GetZ() => _posZ;

        public void SetPosition(float x, float y, float z)
        {
            _posX = x; _posY = y; _posZ = z;
            _dirty = true;
        }

        /// <summary>
        /// Desplaza la posición por un delta escalado por velocidad y tiempo.
        /// </summary>
        public void MoveBy(float dx, float dy, float dz, float speed, float deltaTime)
        {
            _posX += dx * speed * deltaTime;
            _posY += dy * speed * deltaTime;
            _posZ += dz * speed * deltaTime;
            _dirty = true;
        }

        // ---- Rotación (cuaternión) ----

        public float GetRotX() => _rotX;
        public float GetRotY() => _rotY;
        public float GetRotZ() => _rotZ;
        public float GetRotW() => _rotW;

        public void SetRotation(float x, float y, float z, float w)
        {
            _rotX = x; _rotY = y; _rotZ = z; _rotW = w;
            _dirty = true;
        }

        // ---- Vectores derivados (math pura desde cuaternión) ----

        /// <summary>
        /// Vector forward calculado desde el cuaternión de rotación.
        /// Equivale a Transform.forward en Unity.
        /// </summary>
        public (float x, float y, float z) Forward()
        {
            return (
                2f * (_rotX * _rotZ + _rotW * _rotY),
                2f * (_rotY * _rotZ - _rotW * _rotX),
                1f - 2f * (_rotX * _rotX + _rotY * _rotY)
            );
        }

        /// <summary>
        /// Vector right calculado desde el cuaternión de rotación.
        /// Equivale a Transform.right en Unity.
        /// </summary>
        public (float x, float y, float z) Right()
        {
            return (
                1f - 2f * (_rotY * _rotY + _rotZ * _rotZ),
                2f * (_rotX * _rotY + _rotW * _rotZ),
                2f * (_rotX * _rotZ - _rotW * _rotY)
            );
        }

        // ---- Dirty tracking para TransformSyncSystem ----

        public bool IsDirty() => _dirty;
        public void ClearDirty() => _dirty = false;

        // ---- IComponent ----

        public override IComponent Clone()
        {
            return new PositionComponent(_posX, _posY, _posZ, _rotX, _rotY, _rotZ, _rotW);
        }

        public override bool Equivalent(IComponent other)
        {
            return other is PositionComponent o &&
                   Math.Abs(_posX - o._posX) < 0.001f &&
                   Math.Abs(_posY - o._posY) < 0.001f &&
                   Math.Abs(_posZ - o._posZ) < 0.001f;
        }

        public override string ToString()
        {
            return $"PositionComponent({_posX:F2}, {_posY:F2}, {_posZ:F2})";
        }
    }
}
