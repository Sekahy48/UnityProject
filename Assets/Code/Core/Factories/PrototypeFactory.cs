using ECS.Component;
using ECS.Entity;
using Inventory;

namespace Factories
{
    /// <summary>
    /// Creates entity prototypes with pure Core components.
    /// Does NOT add UnityEntityComponent — IEntityLinker does that from Unity.
    /// </summary>
    public static class PrototypeFactory
    {
        public static IEntity CreateResourceNodePrototype()
        {
            ResourceType type = ResourceType.WOOD;
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

        /// <summary>
        /// Creates player prototype with pure Core components.
        /// Position initialized to (0,0,0) — TransformSyncSystem will sync it
        /// with the GameObject's actual Transform after the Link.
        /// </summary>
        public static IEntity CreatePlayerEntityPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId(), "playerEntity");
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            e.AddComponent(new PositionComponent(0f, 0f, 0f));
            e.AddComponent(new InventoryComponent(new InventoryObject()));
            e.AddComponent(new NameComponent("Jugador"));

            var body = new BodyComponent(1.80f, 85, 25, 0);
            e.AddComponent(body);

            var energy = new EnergyComponent(100f, 100f, 100f, 100f);
            energy.CalculateBasalMetabolism(body.GetWeight(), body.GetHeight() * 100f, body.GetAge(), body.GetSex());
            e.AddComponent(energy);

            var nutrition = new NutritionComponent(100f, 100f);
            nutrition.GenerateStoredWater(body.GetWeight(), body.GetSex());
            e.AddComponent(nutrition);

            return e;
        }
    }
}
