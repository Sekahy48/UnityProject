using Observer;
using ECS.Component;
using MVC.Controller;
using System;
using ECS.Entity;
using UnityEngine;

namespace ECS.Systems
{
    public class MovementSystem : IObserver
    {
        private readonly GameContext GameContext;
        public MovementSystem(GameContext gameContext)
        {
            this.GameContext = gameContext;
        }

        public void Update()
        {
            var entitiesWithMovement = GameContext.GetLogic().GetEntitiesWithComponent(typeof(MovementComponent));

            foreach (var entity in entitiesWithMovement)
            {
                MovementComponent movementComponent = entity.GetComponent<MovementComponent>(typeof(MovementComponent));
                if (movementComponent != null)
                {
                    ExecuteMovement(entity, movementComponent);
                }
            }
        }

        private void ExecuteMovement(IEntity entity, MovementComponent movementComponent)
        {

            try
            {
                UnityEntityComponent unityComponent = entity.GetComponent<UnityEntityComponent>(typeof(UnityEntityComponent));
                if (unityComponent == null) throw new EntityComponentException($"UnityEntityComponent not found.");

                Vector2 dir = movementComponent.GetDirection();
                Vector3 move = new Vector3(dir.x, 0f, dir.y); // ejes X y Z

                unityComponent.GetGameObject().transform.position += move * movementComponent.GetSpeed() * Time.deltaTime;
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error executing movement for entity {entity.GetIdAsInt()}: {ex.Message}");
            }

        }
    }
}