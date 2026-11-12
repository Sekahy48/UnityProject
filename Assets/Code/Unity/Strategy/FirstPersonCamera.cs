using Core.ECS.Component;
using Core.ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class FirstPersonCamera : BaseCameraStrategy
    {
        private const float EYE_HEIGHT = 1.7f;
        public FirstPersonCamera(IEntity player)
            : base(player, "FPCamera")
        { 
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
            Rotation.y = Mathf.Clamp(Rotation.y, -90f, 90f);

            PlayerBody.MoveRotation(Quaternion.Euler(0f, Rotation.x, 0f)); 
            Camera.transform.position = PlayerObject.transform.position + Vector3.up * EYE_HEIGHT;
            Camera.transform.rotation = Quaternion.Euler(Rotation.y, Rotation.x, 0f);
        } 
    }
}
