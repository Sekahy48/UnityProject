using System.Collections.Generic;
using Core;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Inventory;
using Core.Item;

namespace Core.Factories
{
    /// <summary>
    /// Creates entity prototypes with pure Core components.
    /// Does NOT add UnityEntityComponent — IEntityLinker does that from Unity.
    /// </summary>
    public class PrototypeFactory
    {
        private ItemCatalogue _itemCatalogue;

        public PrototypeFactory(ItemCatalogue itemCatalogue)
        {
            _itemCatalogue = itemCatalogue;
        }

        public IEntity CreateResourceNodePrototype()
        {
            ResourceType type = ResourceType.Wood;
            var e = new InGameEntity(IdGenerator.GenerateNewId());
            e.AddComponent(new ResourceComponent(type, 0, false));
            return e;
        }

        /// <summary>
        /// Monton de items en el suelo. Nace vacio: lo que contiene lo pone quien lo suelta,
        /// y su posicion la fija quien lo crea. El prototipo solo dice de que piezas esta
        /// hecho.
        /// </summary>
        public IEntity CreateGroundLotPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId());
            e.AddComponent(new GroundLotComponent());

            // Posicion y volumen de partida: la misma presencia en el mundo que un item
            // suelto, de un solo sitio (WorldPresence).
            WorldPresence.AddTo(e);

            return e;
        }

        public IEntity CreateAliveEntityPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId());
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            return e;
        }

        /// <summary>
        /// Creates player prototype with pure Core components.
        /// Position initialized to (0,0,0) — TransformSyncSystem will sync it
        /// with the GameObject's actual Transform after the Link.
        /// </summary>
        public IEntity CreatePlayerEntityPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId());
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            e.AddComponent(new PositionComponent(0f, 0f, 0f));
            e.AddComponent(new GazeComponent());

            var inventory = new InventoryComponent(new InventoryObject(e));
            e.AddComponent(inventory);

            e.AddComponent(StandardEquipment());
            e.AddComponent(new NameComponent("Jugador"));

            var body = new BodyComponent(1.80f, 85, 25, 0);
            e.AddComponent(body);

            var energy = new EnergyComponent(100f, 100f, 100f, 100f);
            energy.CalculateBasalMetabolism(body.Weight, body.Height * 100f, body.Age, body.Sex);
            e.AddComponent(energy);

            var nutrition = new NutritionComponent(100f, 100f);
            nutrition.GenerateStoredWater(body.Weight, body.Sex);
            e.AddComponent(nutrition);

            return e;
        }
    
        public ItemEntity CreateItemFromPrototype(int typeId)
        {
            return _itemCatalogue.CreateItem(typeId);
        } 

        private EquipmentComponent StandardEquipment()
        {
            List<EquipmentSlotType> allowed = new List<EquipmentSlotType>();
            allowed.Add(EquipmentSlotType.Head);
            allowed.Add(EquipmentSlotType.Chest);
            allowed.Add(EquipmentSlotType.Legs);
            allowed.Add(EquipmentSlotType.Feet);

            allowed.Add(EquipmentSlotType.LeftHand);
            allowed.Add(EquipmentSlotType.RightHand);

            
            allowed.Add(EquipmentSlotType.Back);
            allowed.Add(EquipmentSlotType.Hip);
            
            EquipmentComponent component = new EquipmentComponent(allowed);

            component.AddSlot(EquipmentSlotType.Head, 3);
            component.AddSlot(EquipmentSlotType.Chest, 3);
            component.AddSlot(EquipmentSlotType.Legs, 3);
            component.AddSlot(EquipmentSlotType.Feet, 2);

            component.AddSlot(EquipmentSlotType.LeftHand, 1);
            component.AddSlot(EquipmentSlotType.RightHand, 1);
            
            component.AddSlot(EquipmentSlotType.Back, 1);
            component.AddSlot(EquipmentSlotType.Hip, 1); 

            return component;
        }

    }
}
