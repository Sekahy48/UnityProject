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


        }

        public void activate() => Camera.enabled = true;
        public void deactivate() => Camera.enabled = false;

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

            if (Keyboard.current.wKey.isPressed) move += posComp.Forward();
            if (Keyboard.current.sKey.isPressed) move -= posComp.Forward();
            if (Keyboard.current.aKey.isPressed) move -= posComp.Right();
            if (Keyboard.current.dKey.isPressed) move += posComp.Right();

            // Si hay movimiento:
            if (move != Vector3.zero)
            {
                move.Normalize();
                float speed = Keyboard.current.leftShiftKey.isPressed
                    ? movComp.GetSpeed() * movComp.GetRunMultiplier()
                    : movComp.GetSpeed();
                
                Transform newTransform = posComp.ModifyPosition(move, speed, deltaTime);
                this.Camera.transform.position = newTransform.position + new Vector3(0f, 1.6f, 0f);
                

            }
        }
    }






}