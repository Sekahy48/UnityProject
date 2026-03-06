
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

        protected override void HandleMovement(float deltaTime)
        {
            Vector3 move = Vector3.zero;
            PositionComponent posComp = this.GetPos();
            MovementComponent movComp = this.GetMov();

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.wKey.isPressed) { move += posComp.Forward(); vertical += 1f; }
            if (Keyboard.current.sKey.isPressed) { move -= posComp.Forward(); vertical -= 1f; }
            if (Keyboard.current.aKey.isPressed) { move -= posComp.Right(); horizontal -= 1f; }
            if (Keyboard.current.dKey.isPressed) { move += posComp.Right(); horizontal += 1f; }

            // Llamamos al método que gestiona el Animator
            HandleAnimation(horizontal, vertical, deltaTime);

            if (move != Vector3.zero)
            {
                move.Normalize();
                movComp.SetIsJumping(Keyboard.current.spaceKey.isPressed);
                movComp.SetIsRunning(Keyboard.current.leftShiftKey.isPressed);
                float speed = movComp.IsRunning() && movComp.CanRun()
                    ? movComp.GetSpeed() * movComp.GetRunMultiplier()
                    : movComp.GetSpeed();

                posComp.ModifyPosition(move, speed, deltaTime);
            }
            else
            {
                movComp.SetIsRunning(false);
                movComp.SetIsJumping(false);
            }
        }
    }

}