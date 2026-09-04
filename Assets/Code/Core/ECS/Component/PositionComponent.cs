using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Pure C# position component. No UnityEngine dependencies.
    /// TransformSyncSystem (Unity) handles syncing these values
    /// with the GameObject's Transform every frame.
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

        // ---- Position ----

        public float X => _posX;
        public float Y => _posY;
        public float Z => _posZ;

        public void SetPosition(float x, float y, float z)
        {
            _posX = x; _posY = y; _posZ = z;
            _dirty = true;
        }

        /// <summary>
        /// Moves the position by a delta scaled by speed and time.
        /// </summary>
        public void MoveBy(float dx, float dy, float dz, float speed, float deltaTime)
        {
            _posX += dx * speed * deltaTime;
            _posY += dy * speed * deltaTime;
            _posZ += dz * speed * deltaTime;
            _dirty = true;
        }

        // ---- Rotation (quaternion) ----

        public float RotX => _rotX;
        public float RotY => _rotY;
        public float RotZ => _rotZ;
        public float RotW => _rotW;

        public void SetRotation(float x, float y, float z, float w)
        {
            _rotX = x; _rotY = y; _rotZ = z; _rotW = w;
            _dirty = true;
        }

        // ---- Derived vectors (pure math from quaternion) ----

        /// <summary>
        /// Forward vector calculated from the rotation quaternion.
        /// Equivalent to Transform.forward in Unity.
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
        /// Right vector calculated from the rotation quaternion.
        /// Equivalent to Transform.right in Unity.
        /// </summary>
        public (float x, float y, float z) Right()
        {
            return (
                1f - 2f * (_rotY * _rotY + _rotZ * _rotZ),
                2f * (_rotX * _rotY + _rotW * _rotZ),
                2f * (_rotX * _rotZ - _rotW * _rotY)
            );
        }

        // ---- Dirty tracking for TransformSyncSystem ----

        public bool IsDirty => _dirty;
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
