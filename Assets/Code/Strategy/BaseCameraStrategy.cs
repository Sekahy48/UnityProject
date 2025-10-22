using System;
using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public abstract class BaseCameraStrategy : ICameraStrategy, IObserver
    {
        protected Camera Camera;
        protected GameObject PlayerObject;
        protected IEntity player;
        protected Animator animator;

        protected Vector2 rotation = Vector2.zero;

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

        protected internal PositionComponent GetPos() => player.GetComponent<PositionComponent>(typeof(PositionComponent));
        protected internal MovementComponent GetMov() => player.GetComponent<MovementComponent>(typeof(MovementComponent));
        public IEntity GetPlayer() => player; 
        public abstract void Execute(float deltaTime);
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

        public Camera GetCamera() => Camera;
    }
}
