#if false
using System;
using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class TPCameraStrategy : ICameraStrategy, IObserver
    {
        private Camera Camera;
        private GameObject PlayerObject;
        private IEntity player;
        private Animator animator;
        
        // Parámetros de cámara TPC
        private float distance = 3.0f;     // Distancia detrás del jugador
        private float height = 1.8f;       // Altura respecto al jugador
        private float rotationSmoothness = 10f;
        private Vector2 rotation = Vector2.zero;

        public TPCameraStrategy(IEntity player)
        {
            this.player = player;
            this.PlayerObject = GameObject.FindWithTag("MainPlayer");
            this.Camera = new GameObject("TPCamera").AddComponent<Camera>();

            // Aseguramos que el transform del jugador esté vinculado
            this.GetPos().SetTransform(PlayerObject.transform);

            // Posición inicial de la cámara
            this.Camera.transform.position = PlayerObject.transform.position + new Vector3(0f, height, -distance);
            this.Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * height);
            this.animator = PlayerObject.GetComponent<Animator>();
        }

        public void Activate() => Camera.enabled = true;
        public void Deactivate() => Camera.enabled = false;

        private PositionComponent GetPos() => player.GetComponent<PositionComponent>(typeof(PositionComponent));
        private MovementComponent GetMov() => player.GetComponent<MovementComponent>(typeof(MovementComponent));

        public void Execute(float deltaTime)
        {
            this.HandleMouseLook(deltaTime);
            this.HandleMovement(deltaTime);
        }

        public void Update() => throw new NotImplementedException();

        private void HandleMouseLook(float deltaTime)
        {
            MovementComponent movComp = this.GetMov();
            PositionComponent posComp = this.GetPos();

            // Delta del ratón
            float mouseX = Mouse.current.delta.x.ReadValue() * movComp.GetMouseSensitivity();
            float mouseY = Mouse.current.delta.y.ReadValue() * movComp.GetMouseSensitivity();

            // Acumulamos la rotación
            rotation.x += mouseX;
            rotation.y -= mouseY;
            rotation.y = Mathf.Clamp(rotation.y, -45f, 75f); // limitar ángulo vertical

            // Calculamos la rotación en espacio mundial
            Quaternion cameraRotation = Quaternion.Euler(rotation.y, rotation.x, 0f);

            // Posición deseada detrás del jugador
            Vector3 targetPosition = PlayerObject.transform.position
                - cameraRotation * Vector3.forward * distance
                + Vector3.up * height;

            // Suavizamos el movimiento
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, targetPosition, deltaTime * rotationSmoothness);

            // La cámara siempre mira al jugador
            Camera.transform.LookAt(PlayerObject.transform.position + Vector3.up * height);

            // Si quieres que el jugador rote con la cámara (opcional)
            posComp.GetTransform().rotation = Quaternion.Euler(0f, rotation.x, 0f);
        }

        private void HandleMovement(float deltaTime)
        {
            Vector3 move = Vector3.zero;
            PositionComponent posComp = this.GetPos();
            MovementComponent movComp = this.GetMov();

            //movComp.ResetJump();
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
            //Debug.Log("IsRunning: " + GetMov().IsRunning());
            animator.SetBool("IsRunning", GetMov().IsRunning());

            //Debug.Log("IsJumping: " + GetMov().IsMoving());
            animator.SetBool("IsJumping", this.GetMov().IsJumping());
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