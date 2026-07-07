using System;
using ECS.Component;
using ECS.Entity;
using Inventory;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace Factories
{
    public static class PrototypeFactory
    {
        // ---- Prototypes ----

        public static IEntity CreateResourceNodePrototype()
        {
            ResourceType type = ResourceType.WOOD; // Cambia el tipo de recurso según sea necesario
            var e = new InGameEntity(IdGenerator.GenerateNewId(), "resourceNode"); 
            e.AddComponent(new ResourceComponent(type, 0, false));
            return e;
        }

        public static IEntity CreateAliveEntityPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId(), "aliveEntity"); 
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            return e;
        }

        public static IEntity CreatePlayerEntityPrototype(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("Player suministrado para creación de entidad de Jugador nula.");
                throw new NullReferenceException("Player is null.");
            }else if(player.transform == null)
            {
                Debug.LogError("El GameObject suministrado contiene un transform aparentemente nulo.");
                throw new ArgumentException("Player's transform is not valid.");
            }

            var e = new InGameEntity(IdGenerator.GenerateNewId(), "playerEntity"); 
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            e.AddComponent(new PositionComponent(player.transform));
            e.AddComponent(new InventoryComponent(new InventoryObject()));
            e.AddComponent(new NameComponent("Jugador"));
            e.AddComponent(new FisiologicComponent(1.80f, 85, 25, 0)); 
            if (player == null)
            {
                Debug.LogError("Player suministrado para creación de entidad de Jugador nula.");
                throw new NullReferenceException("Player is null.");
            }
            e.AddComponent(new UnityEntityComponent(player)); // La asignaremos luego
            return e;
        }
    }
}
