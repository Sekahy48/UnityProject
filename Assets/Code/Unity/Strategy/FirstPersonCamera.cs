using ECS.Component;
using ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class FirstPersonCamera : BaseCameraStrategy
    {
        public FirstPersonCamera(IEntity player)
            : base(player, "FPCamera")
        {
            Camera.transform.SetParent(PlayerObject.transform);
            Camera.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            Camera.transform.localRotation = Quaternion.identity;
        }

        public override void Update() { }

        protected override void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = GetMov();

            float mouseX = Mouse.current.delta.x.ReadValue() * movComp.GetMouseSensitivity();
            float mouseY = Mouse.current.delta.y.ReadValue() * movComp.GetMouseSensitivity();

            rotation.x += mouseX;
            rotation.y -= mouseY;
            rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);

            PlayerObject.transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);
            Camera.transform.localRotation = Quaternion.Euler(rotation.y, 0f, 0f);
        }

        // HandleMovement inherited from BaseCameraStrategy
    }
}
