
using Core.ECS.Component;
using Core.ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class ThirdPersonCamera : BaseCameraStrategy
    {
        private float _distance = 3.0f;
        private float _height = 1.8f;
        private float _rotationSmoothness = 10f;
        private Vector3 _offset;

        public ThirdPersonCamera(IEntity player)
            : base(player, "TPCamera")
        {
            Camera.transform.position = PlayerObject.transform.position + new Vector3(0f, _height, -_distance);
            Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * _height);
            _offset = Camera.transform.position - PlayerObject.transform.position;
        }

        public override void Update() { }

        protected override void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = GetMov();

            Vector2 delta = ReadLookDelta();
            float mouseX = delta.x * movComp.MouseSensitivity;
            float mouseY = delta.y * movComp.MouseSensitivity;

            Rotation.x += mouseX;
            Rotation.y -= mouseY;
            Rotation.y = Mathf.Clamp(Rotation.y, -45f, 75f);

            Quaternion cameraRotation = Quaternion.Euler(Rotation.y, Rotation.x, 0f);

            Vector3 targetOffset = -(cameraRotation * Vector3.forward) * _distance + Vector3.up * _height;
            _offset = Vector3.Lerp(_offset, targetOffset, deltaTime * _rotationSmoothness);
            Camera.transform.position = PlayerObject.transform.position + _offset;
            Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * _height);
 
            PlayerBody.MoveRotation(Quaternion.Euler(0f, Rotation.x, 0f));
        }
 
    }
}
