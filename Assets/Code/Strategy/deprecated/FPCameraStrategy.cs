using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements; // 👈 Importante

namespace Strategy
{

    public class FPCameraStrategy : ICameraStrategy, IObserver
    {

        // TEMPORAL
        private GameObject playerObject = GameObject.FindWithTag("MainPlayer");
        // TEMPORAL
        private readonly Camera camera;
        private Transform playerTransform;

        private float moveSpeed = 5f;
        private float runMultiplier = 2f;
        private float mouseSensitivity = 1f;
        private IEntity player;

        private float xRotation = 0f;

        public FPCameraStrategy(IEntity player)
        {
            this.player = player;

            PositionComponent pos = this.GetPos(); 
            MovementComponent mov = this.GetMov();
            pos.Attach(this);
            mov.Attach(this);

            // Aseguramos la relación entre el PositionComponent y el Transform del GameObject
            pos.SetTransform(playerObject.transform);

            this.camera = new GameObject("FPCamera").AddComponent<Camera>();

            this.playerTransform = GetPos().GetTransform();

            // Posicionar la cámara en el jugador y hacerla hija de este
            Transform tCamera = camera.transform;
            tCamera.SetParent(playerTransform);
            tCamera.position = playerTransform.position + new Vector3(0f, 1.6f, 0f); // altura de ojos aprox 
            tCamera.rotation = playerTransform.rotation;
            // Los componentes de movimiento y posición del jugador deberían notificar a esta cámara
            
            this.Update();
        }

        private PositionComponent GetPos()
        {
            return player.GetComponent<PositionComponent>(typeof(PositionComponent));
        }

        private MovementComponent GetMov()
        {
            return player.GetComponent<MovementComponent>(typeof(MovementComponent));
        }
        
        public void activate() => camera.enabled = true;
        public void deactivate() => camera.enabled = false;

        public void Execute(float deltaTime)
        {
            HandleMouseLook(deltaTime);
            HandleMovement(deltaTime);
        }

        private void HandleMouseLook(float deltaTime)
        {
            // Delta del ratón
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity; 

            // Rotación en eje Y (horizontal) al cuerpo
            playerTransform.Rotate(Vector3.up * mouseX);

            // Rotación en eje X (vertical) solo la cámara
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        private void HandleMovement(float deltaTime)
        {
            Vector3 move = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) move += playerTransform.forward;
            if (Keyboard.current.sKey.isPressed) move -= playerTransform.forward;
            if (Keyboard.current.aKey.isPressed) move -= playerTransform.right;
            if (Keyboard.current.dKey.isPressed) move += playerTransform.right;

            if (move != Vector3.zero)
            {
                move.Normalize();
                float speed = Keyboard.current.leftShiftKey.isPressed
                    ? moveSpeed * runMultiplier
                    : moveSpeed;
                this.camera.transform.position = playerTransform.position + new Vector3(0f, 1.6f, 0f);
                playerTransform.position += move * speed * deltaTime;
                
            }
        }

        public void Update()
        {
            // Actualizar respecto al PositionComponent
            if (this.playerTransform != GetPos().GetTransform())
            {
                this.playerTransform = GetPos().GetTransform();
                Debug.LogWarning("Información referente a la posicion del jugador no coherente. Actualizando referencia. ¡Aviso! Las consecuencias de esta situación son impredecibles. Si ocurre es posible que la cámara no este enlazada al jugador.");
            }

            // Actualizar respecto al MovementComponent
            MovementComponent mov = GetMov();
            if (mov != null)
            {
                this.runMultiplier = mov.GetRunMultiplier();
                this.moveSpeed = mov.GetSpeed();
            }
            else
            {
                Debug.LogError("MovementComponent no encontrado en el jugador. No se puede actualizar la cámara en primera persona.");
            }
        }
    }
}
