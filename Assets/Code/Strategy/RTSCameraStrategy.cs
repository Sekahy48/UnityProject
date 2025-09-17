using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class RTSCameraStrategy : ICameraStrategy
    {
        private readonly Camera camera;
        private float cameraSpeed = 10f;
        private float rotationSpeed = 45f;

        // Punto alrededor del que orbitamos
        private Vector3 pivot;

        public RTSCameraStrategy()
        {
            GameObject camGO = new GameObject("RTSCamera");
            camGO.transform.position = new Vector3(0f, 20f, -20f);
            camGO.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            this.camera = camGO.AddComponent<Camera>();

            pivot = new Vector3(0f, 0f, 0f); // al principio, que mire al origen
        }

        public void activate() => camera.enabled = true;
        public void deactivate() => camera.enabled = false;

        public void Execute(float deltaTime)
        {
            Transform camTransform = camera.transform;

            // Actualizar pivot dinámicamente: lo que "mira" la cámara a cierta distancia
            pivot = camTransform.position + camTransform.forward * 10f;

            // -------- SCROLL RUEDA RATÓN --------
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll != 0f)
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    // Subir/bajar altura MÁS RÁPIDO (3x) y dirección invertida
                    float verticalSpeed = 10f * 3f; // 3 veces más rápido
                    camTransform.Translate(Vector3.up * -scroll * deltaTime * verticalSpeed, Space.World);
                    pivot += Vector3.up * -scroll * deltaTime * verticalSpeed; // ajustar pivot
                }
                else
                {
                    // Zoom (ajustar FOV)
                    camera.fieldOfView -= scroll * deltaTime * 10f;
                }
            }
            // -----------------------------------

            // Velocidad normal / boost
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                cameraSpeed = 30f;
                rotationSpeed = 135f;
            }
            else
            {
                cameraSpeed = 10f;
                rotationSpeed = 45f;
            }

            Vector3 forward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            Vector3 right   = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

            // Movimiento básico
            if (Keyboard.current.wKey.isPressed) camTransform.position += forward * cameraSpeed * deltaTime;
            if (Keyboard.current.sKey.isPressed) camTransform.position -= forward * cameraSpeed * deltaTime;
            if (Keyboard.current.aKey.isPressed) camTransform.position -= right * cameraSpeed * deltaTime;
            if (Keyboard.current.dKey.isPressed) camTransform.position += right * cameraSpeed * deltaTime;

            // Movimiento con drag (botón central)
            if (Mouse.current.middleButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                Vector3 lateral = right   * -deltaX * cameraSpeed * deltaTime / 20f;
                Vector3 frontal = forward * -deltaY * cameraSpeed * deltaTime / 20f;

                camTransform.position += lateral + frontal;
                pivot += lateral + frontal; // mover también el pivot
            }

            // Orbitación con Q/E
            if (Keyboard.current.qKey.isPressed)
                OrbitAroundPivot(Vector3.up, rotationSpeed * deltaTime);
            if (Keyboard.current.eKey.isPressed)
                OrbitAroundPivot(Vector3.up, -rotationSpeed * deltaTime);

            // Flechas izquierda/derecha
            if (Keyboard.current.leftArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, -rotationSpeed * deltaTime);
            if (Keyboard.current.rightArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, rotationSpeed * deltaTime);

            if (Keyboard.current.upArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, -rotationSpeed * deltaTime);
            if (Keyboard.current.downArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, rotationSpeed * deltaTime);

            // Orbitación con click derecho + drag
            if (Mouse.current.rightButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                // Si llegamos al borde de pantalla, simulamos movimiento extra
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x <= 5) deltaX = -5;
                if (mousePos.x >= Screen.width - 5) deltaX = 5;
                if (mousePos.y <= 5) deltaY = -5;
                if (mousePos.y >= Screen.height - 5) deltaY = 5;

                OrbitAroundPivot(Vector3.up, deltaX * 0.2f);
                OrbitAroundPivot(camTransform.right, -deltaY * 0.2f);
            }

            // Clamp final de FOV
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 30f, 100f);
        }

        private void OrbitAroundPivot(Vector3 axis, float angle)
        {
            Transform camTransform = camera.transform;
            camTransform.RotateAround(pivot, axis, angle);
        }
    }
}
