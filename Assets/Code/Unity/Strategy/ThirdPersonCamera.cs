
using ECS.Component;
using ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class ThirdPersonCamera : BaseCameraStrategy
    {
        private float distance = 3.0f;
        private float height = 1.8f;
        private float rotationSmoothness = 10f;

        public ThirdPersonCamera(IEntity player)
            : base(player, "TPCamera")
        {
            Camera.transform.position = PlayerObject.transform.position + new Vector3(0f, height, -distance);
            Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * height);
        }

        public override void Update() { }

        protected override void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = GetMov();

            float mouseX = Mouse.current.delta.x.ReadValue() * movComp.GetMouseSensitivity();
            float mouseY = Mouse.current.delta.y.ReadValue() * movComp.GetMouseSensitivity();

            rotation.x += mouseX;
            rotation.y -= mouseY;
            rotation.y = Mathf.Clamp(rotation.y, -45f, 75f);

            Quaternion cameraRotation = Quaternion.Euler(rotation.y, rotation.x, 0f);

            Vector3 targetPosition = PlayerObject.transform.position
                - cameraRotation * Vector3.forward * distance
                + Vector3.up * height;

            Camera.transform.position = Vector3.Lerp(Camera.transform.position, targetPosition, deltaTime * rotationSmoothness);
            Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * height);

            // Rotate the player directly via Transform (not via PositionComponent)
            PlayerObject.transform.rotation = Quaternion.Euler(0f, rotation.x, 0f);
        }

        // HandleMovement inherited from BaseCameraStrategy
    }
}
