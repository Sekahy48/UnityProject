using Core.ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class RTSCameraStrategy : ICameraStrategy
    {
        private readonly Camera _camera;
        private float _cameraSpeed = 10f;
        private float _rotationSpeed = 45f;

        // Punto alrededor del cual se orbita
        private Vector3 _pivot;

        public RTSCameraStrategy()
        {
            GameObject camGO = new GameObject("RTSCamera");
            camGO.transform.position = new Vector3(0f, 20f, -20f);
            camGO.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            _camera = camGO.AddComponent<Camera>();

            _pivot = new Vector3(0f, 0f, 0f); // al principio, mira al origen
        }

        public void Activate() => _camera.enabled = true;
        public void Deactivate() => _camera.enabled = false;

        public void Execute(float deltaTime)
        {
            Transform camTransform = _camera.transform;

            // Pivote dinamico: lo que la camara "mira" a cierta distancia
            _pivot = camTransform.position + camTransform.forward * 10f;

            // -------- RUEDA DEL RATON --------
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll != 0f)
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    // Subir o bajar la altura MAS RAPIDO (x3) y con la direccion invertida
                    float verticalSpeed = 10f * 3f; // 3 veces mas rapido
                    camTransform.Translate(Vector3.up * -scroll * deltaTime * verticalSpeed, Space.World);
                    _pivot += Vector3.up * -scroll * deltaTime * verticalSpeed; // ajusta el pivote
                }
                else
                {
                    // Zoom (ajusta el FOV)
                    _camera.fieldOfView -= scroll * deltaTime * 10f;
                }
            }
            // -----------------------------------

            // Velocidad normal / acelerada
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                _cameraSpeed = 30f;
                _rotationSpeed = 135f;
            }
            else
            {
                _cameraSpeed = 10f;
                _rotationSpeed = 45f;
            }

            Vector3 forward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            Vector3 right   = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

            // Movimiento basico
            if (Keyboard.current.wKey.isPressed) camTransform.position += forward * _cameraSpeed * deltaTime;
            if (Keyboard.current.sKey.isPressed) camTransform.position -= forward * _cameraSpeed * deltaTime;
            if (Keyboard.current.aKey.isPressed) camTransform.position -= right * _cameraSpeed * deltaTime;
            if (Keyboard.current.dKey.isPressed) camTransform.position += right * _cameraSpeed * deltaTime;

            // Movimiento arrastrando (boton central)
            if (Mouse.current.middleButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                Vector3 lateral = right   * -deltaX * _cameraSpeed * deltaTime / 20f;
                Vector3 frontal = forward * -deltaY * _cameraSpeed * deltaTime / 20f;

                camTransform.position += lateral + frontal;
                _pivot += lateral + frontal; // mueve tambien el pivote
            }

            // Orbitar con Q/E
            if (Keyboard.current.qKey.isPressed)
                OrbitAroundPivot(Vector3.up, _rotationSpeed * deltaTime);
            if (Keyboard.current.eKey.isPressed)
                OrbitAroundPivot(Vector3.up, -_rotationSpeed * deltaTime);

            // Flechas izquierda/derecha
            if (Keyboard.current.leftArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, -_rotationSpeed * deltaTime);
            if (Keyboard.current.rightArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, _rotationSpeed * deltaTime);

            if (Keyboard.current.upArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, -_rotationSpeed * deltaTime);
            if (Keyboard.current.downArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, _rotationSpeed * deltaTime);

            // Orbitar con clic derecho + arrastrar
            if (Mouse.current.rightButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                // Si se llega al borde de la pantalla, se simula movimiento extra
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x <= 5) deltaX = -5;
                if (mousePos.x >= Screen.width - 5) deltaX = 5;
                if (mousePos.y <= 5) deltaY = -5;
                if (mousePos.y >= Screen.height - 5) deltaY = 5;

                OrbitAroundPivot(Vector3.up, deltaX * 0.2f);
                OrbitAroundPivot(camTransform.right, -deltaY * 0.2f);
            }

            // Limite final del FOV
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView, 30f, 100f);
        }

        private void OrbitAroundPivot(Vector3 axis, float angle)
        {
            Transform camTransform = _camera.transform;
            camTransform.RotateAround(_pivot, axis, angle);
        }

        public Camera GetCamera() => _camera;
    }
}
