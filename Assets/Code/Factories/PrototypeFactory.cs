using System;
using ECS.Component;
using ECS.Entity;
using Unity.VisualScripting;
using UnityEngine;

namespace Factories
{
    public static class PrototypeFactory
    {
        // ---- Prototypes ----

        public static IEntity CreateResourceNodePrototype()
        {
            ResourceType type = ResourceType.WOOD; // Cambia el tipo de recurso según sea necesario
            var e = new UnaliveEntity(IdGenerator.GenerateNewId(), "resourceNode"); 
            e.AddComponent(new ResourceComponent(type, 0, false));
            return e;
        }

        public static IEntity CreateAliveEntityPrototype()
        {
            var e = new AliveEntity(IdGenerator.GenerateNewId(), "aliveEntity"); 
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            return e;
        }

        public static IEntity CreatePlayerEntityPrototype()
        {
            var e = new AliveEntity(IdGenerator.GenerateNewId(), "playerEntity"); 
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            e.AddComponent(new PositionComponent(0, 0, 0));
            e.AddComponent(new InventoryComponent(36));
            e.AddComponent(new NameComponent("Jugador"));
            e.AddComponent(new FisiologicComponent(1.80f, 85, 25, 0));

            var hierarchyPlayer = GameObject.FindGameObjectWithTag("Player");
            if (hierarchyPlayer == null)
            {
                Debug.LogError("No se ha encontrado el GameObject con la etiqueta 'Player'. Asegúrate de que existe en la escena.");
                throw new NullReferenceException("GameObject with tag 'Player' not found.");
            }
            e.AddComponent(new UnityEntityComponent(hierarchyPlayer)); // La asignaremos luego
            return e;
        }
    }
}
