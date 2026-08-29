using ECS.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategy
{
    public class RTSCameraStrategy : ICameraStrategy
    {
        private readonly Camera Camera;
        private float cameraSpeed = 10f;
        private float rotationSpeed = 45f;

        // Point we orbit around
        private Vector3 pivot;

        public RTSCameraStrategy()
        {
            GameObject camGO = new GameObject("RTSCamera");
            camGO.transform.position = new Vector3(0f, 20f, -20f);
            camGO.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            this.Camera = camGO.AddComponent<Camera>();

            pivot = new Vector3(0f, 0f, 0f); // initially, look at the origin
        }

        public void Activate() => Camera.enabled = true;
        public void Deactivate() => Camera.enabled = false;

        public void Execute(float deltaTime)
        {
            Transform camTransform = Camera.transform;

            // Dynamically update pivot: what the camera "looks at" from a certain distance
            pivot = camTransform.position + camTransform.forward * 10f;

            // -------- MOUSE WHEEL SCROLL --------
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll != 0f)
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    // Raise/lower height FASTER (3x) and inverted direction
                    float verticalSpeed = 10f * 3f; // 3 times faster
                    camTransform.Translate(Vector3.up * -scroll * deltaTime * verticalSpeed, Space.World);
                    pivot += Vector3.up * -scroll * deltaTime * verticalSpeed; // adjust pivot
                }
                else
                {
                    // Zoom (adjust FOV)
                    Camera.fieldOfView -= scroll * deltaTime * 10f;
                }
            }
            // -----------------------------------

            // Normal speed / boost
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

            // Basic movement
            if (Keyboard.current.wKey.isPressed) camTransform.position += forward * cameraSpeed * deltaTime;
            if (Keyboard.current.sKey.isPressed) camTransform.position -= forward * cameraSpeed * deltaTime;
            if (Keyboard.current.aKey.isPressed) camTransform.position -= right * cameraSpeed * deltaTime;
            if (Keyboard.current.dKey.isPressed) camTransform.position += right * cameraSpeed * deltaTime;

            // Drag movement (middle button)
            if (Mouse.current.middleButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                Vector3 lateral = right   * -deltaX * cameraSpeed * deltaTime / 20f;
                Vector3 frontal = forward * -deltaY * cameraSpeed * deltaTime / 20f;

                camTransform.position += lateral + frontal;
                pivot += lateral + frontal; // also move the pivot
            }

            // Orbit with Q/E
            if (Keyboard.current.qKey.isPressed)
                OrbitAroundPivot(Vector3.up, rotationSpeed * deltaTime);
            if (Keyboard.current.eKey.isPressed)
                OrbitAroundPivot(Vector3.up, -rotationSpeed * deltaTime);

            // Left/right arrows
            if (Keyboard.current.leftArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, -rotationSpeed * deltaTime);
            if (Keyboard.current.rightArrowKey.isPressed)
                OrbitAroundPivot(Vector3.up, rotationSpeed * deltaTime);

            if (Keyboard.current.upArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, -rotationSpeed * deltaTime);
            if (Keyboard.current.downArrowKey.isPressed)
                OrbitAroundPivot(camTransform.right, rotationSpeed * deltaTime);

            // Orbit with right click + drag
            if (Mouse.current.rightButton.isPressed)
            {
                float deltaX = Mouse.current.delta.x.ReadValue();
                float deltaY = Mouse.current.delta.y.ReadValue();

                // If we reach the screen edge, simulate extra movement
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos.x <= 5) deltaX = -5;
                if (mousePos.x >= Screen.width - 5) deltaX = 5;
                if (mousePos.y <= 5) deltaY = -5;
                if (mousePos.y >= Screen.height - 5) deltaY = 5;

                OrbitAroundPivot(Vector3.up, deltaX * 0.2f);
                OrbitAroundPivot(camTransform.right, -deltaY * 0.2f);
            }

            // Final FOV clamp
            Camera.fieldOfView = Mathf.Clamp(Camera.fieldOfView, 30f, 100f);
        }

        private void OrbitAroundPivot(Vector3 axis, float angle)
        {
            Transform camTransform = Camera.transform;
            camTransform.RotateAround(pivot, axis, angle);
        }

        public Camera GetCamera() => Camera;
    }
}
