using UnityEngine;
using UnityEngine.InputSystem; // 👈 Importante

namespace Strategy
{

    public class FPCameraStrategy : ICameraStrategy
    {

        // TEMPORAL
        private GameObject playerObject = GameObject.FindWithTag("MainPlayer");
        // TEMPORAL
        private readonly Camera camera;
        private readonly Transform playerTransform;

        private float moveSpeed = 5f;
        private float runMultiplier = 2f;
        private float mouseSensitivity = 2f;

        private float xRotation = 0f;

        public FPCameraStrategy(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
            this.camera = new GameObject("FPCamera").AddComponent<Camera>();

            // 👇 asegura que la cámara sea hija del jugador (estilo FPS clásico)
            camera.transform.SetParent(playerTransform);
            camera.transform.localPosition = new Vector3(0f, 1.6f, 0f); // altura de ojos aprox
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
            // 🖱️ Nuevo Input System: delta del ratón
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * deltaTime;

            // Rotación en eje Y (horizontal) → al cuerpo
            playerTransform.Rotate(Vector3.up * mouseX);

            // Rotación en eje X (vertical) → solo la cámara
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

                playerTransform.position += move * speed * deltaTime;
            }
        }
    }
}
