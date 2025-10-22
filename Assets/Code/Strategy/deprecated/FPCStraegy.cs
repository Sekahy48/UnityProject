#if false
using System;
using ECS.Component;
using ECS.Entity;
using Observer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class FPCStrategy : ICameraStrategy, IObserver
    {

        private Camera Camera;
        private GameObject PlayerObject;
        private IEntity player;
        private Animator animator;


        public FPCStrategy(IEntity player)
        {

            this.player = player;
            this.PlayerObject = GameObject.FindWithTag("MainPlayer");
            this.Camera = new GameObject("FPCamera").AddComponent<Camera>();

            // Como el Clone del componenete de posicion ignora el Transform que le pasamos en el constructor se lo tenemos que volver a asignar
            this.GetPos().SetTransform(PlayerObject.transform);
            // Ligamos la camara al jugador
            Transform PlayerTransform = GetPos().GetTransform();
            this.Camera.transform.SetParent(PlayerTransform);
            this.Camera.transform.position = PlayerTransform.position + new Vector3(0f, 1.6f, 0f); // altura de ojos aprox
            this.Camera.transform.rotation = PlayerTransform.rotation;
            this.animator = PlayerObject.GetComponent<Animator>();  

        }

        public void Activate() => Camera.enabled = true;
        public void Deactivate() => Camera.enabled = false;

        private PositionComponent GetPos() {
            return player.GetComponent<PositionComponent>(typeof(PositionComponent));
        }

        private MovementComponent GetMov() {
            return player.GetComponent<MovementComponent>(typeof(MovementComponent));
        }


        public void Execute(float deltaTime)
        {
            this.HandleMouseLook(deltaTime);
            this.HandleMovement(deltaTime);
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

         private void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = this.GetMov();
            PositionComponent posComp = this.GetPos();

            // Delta del ratón
            float mouseX = Mouse.current.delta.x.ReadValue() * movComp.GetMouseSensitivity();
            float mouseY = Mouse.current.delta.y.ReadValue() * movComp.GetMouseSensitivity();

            // Rotación dcha a izq  que el ratón se mueva (Eje Y)
            posComp.ModifyRotation(new Vector3(0f, mouseX, 0f));

            // Rotación arriba y abajo (Eje X)
            posComp.ModifyXRotation(-mouseY, this.Camera);
        }

        private void HandleMovement(float deltaTime)
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

            // 🔹 Llamamos al método que gestiona el Animator
            HandleAnimation(horizontal, vertical, deltaTime);

            if (move != Vector3.zero)
            {
                move.Normalize();
                movComp.SetIsRunning(Keyboard.current.leftShiftKey.isPressed);
                float speed = movComp.IsRunning()
                    ? movComp.GetSpeed() * movComp.GetRunMultiplier()
                    : movComp.GetSpeed();

                posComp.ModifyPosition(move, speed, deltaTime);
            }
        }


        private void HandleAnimation(float horizontal, float vertical, float deltaTime)
        {
            if (animator == null)
                return;

            // Comprobación de si el personaje está corriendo
            Debug.Log("IsRunning: " + GetMov().IsRunning());
            animator.SetBool("IsRunning", GetMov().IsRunning());

            // Suavizado del cambio de valores (imitando Input.GetAxis)
            float currentHorizontal = animator.GetFloat("VelX");
            float currentVertical = animator.GetFloat("VelY");

            float smoothHorizontal = Mathf.Lerp(currentHorizontal, horizontal, deltaTime * 10f);
            float smoothVertical = Mathf.Lerp(currentVertical, vertical, deltaTime * 10f);

            animator.SetFloat("VelX", smoothHorizontal);
            animator.SetFloat("VelY", smoothVertical);
        }

        public Camera GetCamera() => Camera;

    }






}
#endif