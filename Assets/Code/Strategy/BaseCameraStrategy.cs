using System;
using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public abstract class BaseCameraStrategy : ICameraStrategy, IObserver, IInventoryInputSource
    {
        protected Camera Camera;
        protected GameObject PlayerObject;
        protected IEntity player;
        protected Animator animator;

        protected Vector2 rotation = Vector2.zero;

        public event Action<int> OnInventoryRequested;
        protected BaseCameraStrategy(IEntity player, string cameraName)
        {
            Debug.Log("Camera created: " + cameraName);
            this.player = player;
            this.PlayerObject = GameObject.FindWithTag("MainPlayer");
            this.Camera = new GameObject(cameraName).AddComponent<Camera>();
            this.animator = PlayerObject.GetComponent<Animator>();
            this.GetPos().SetTransform(PlayerObject.transform);
        }

        // Métodos comunes
        public virtual void Activate() => Camera.enabled = true;
        public virtual void Deactivate() => Camera.enabled = false;

        protected internal PositionComponent GetPos() => player.GetComponent<PositionComponent>();
        protected internal MovementComponent GetMov() => player.GetComponent<MovementComponent>();
        public IEntity GetPlayer() => player; 
        public void Execute(float deltaTime)
        {
            HandleMouseLook(deltaTime);
            HandleMovement(deltaTime);
            HandleInventoryInput();
        }

        public abstract void Update();
 
        protected virtual void HandleAnimation(float horizontal, float vertical, float deltaTime)
        {
            //Debug.Log("IsRunning al empezar la animacion: " + GetMov().IsRunning());
            if (animator == null)
                return;

            var mov = GetMov();
            animator.SetBool("IsRunning", mov.IsRunning());
            animator.SetBool("IsJumping", mov.IsJumping());

            float smoothHorizontal = Mathf.Lerp(animator.GetFloat("VelX"), horizontal, deltaTime * 10f);
            float smoothVertical = Mathf.Lerp(animator.GetFloat("VelY"), vertical, deltaTime * 10f);

            animator.SetFloat("VelX", smoothHorizontal);
            animator.SetFloat("VelY", smoothVertical);
            //Debug.Log("IsRunning al acabar la animacion: " + GetMov().IsRunning());
        }

        protected void HandleInventoryInput()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            { 
                Debug.Log("Main Inventory key pressed. Toggle inventory UI."); 
                OnInventoryRequested?.Invoke(1);
            } else if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("Personal Area Inventory key pressed. Toggle inventory UI."); 
                OnInventoryRequested?.Invoke(0);
            } else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("Escape key pressed. Hidding inventory UI."); 
                OnInventoryRequested?.Invoke(-1);
             }
        }

        public Camera GetCamera() => Camera;

        protected abstract void HandleMouseLook(float deltaTime);

        protected virtual void HandleMovement(float deltaTime)
        {
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
