
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

        public override void Execute(float deltaTime)
        {
            HandleMouseLook(deltaTime);
            HandleMovement(deltaTime);
        }

        public override void Update() { }

        private void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = GetMov();
            PositionComponent posComp = GetPos();

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

            posComp.GetTransform().rotation = Quaternion.Euler(0f, rotation.x, 0f);
        }

        private void HandleMovement(float deltaTime)
        {
            //Debug.Log("IsRunning al empezar: " + GetMov().IsRunning());
            Vector3 move = Vector3.zero;
            var posComp = GetPos();
            var movComp = GetMov();

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.wKey.isPressed) { move += posComp.Forward(); vertical += 1f; }
            if (Keyboard.current.sKey.isPressed) { move -= posComp.Forward(); vertical -= 1f; }
            if (Keyboard.current.aKey.isPressed) { move -= posComp.Right(); horizontal -= 1f; }
            if (Keyboard.current.dKey.isPressed) { move += posComp.Right(); horizontal += 1f; }

            HandleAnimation(horizontal, vertical, deltaTime);

            if (move != Vector3.zero)
            {
                move.Normalize();
                movComp.SetIsJumping(Keyboard.current.spaceKey.isPressed);
                movComp.SetIsRunning(Keyboard.current.leftShiftKey.isPressed);
                //Debug.Log("CanRun: " + movComp.CanRun());
                float speed = movComp.IsRunning()
                    ? movComp.GetSpeed() * movComp.GetRunMultiplier()
                    : movComp.GetSpeed();

                posComp.ModifyPosition(move, speed, deltaTime);
            }
            else
            {
                movComp.SetIsRunning(false);
                movComp.SetIsJumping(false);
            }
            //Debug.Log("IsRunning al acabar: " + GetMov().IsRunning());
        }
    }
}