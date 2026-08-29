using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Component.Equipment; 
using ECS.Entity;
using Inventory;
using Item;

namespace Factories
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
            var e = new InGameEntity(IdGenerator.GenerateNewId(), "resourceNode");
            e.AddComponent(new ResourceComponent(type, 0, false));
            return e;
        }

        public IEntity CreateAliveEntityPrototype()
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
        public IEntity CreatePlayerEntityPrototype()
        {
            var e = new InGameEntity(IdGenerator.GenerateNewId(), "playerEntity");
            e.AddComponent(new HealthComponent(100));
            e.AddComponent(new MovementComponent(2.0f));
            e.AddComponent(new PositionComponent(0f, 0f, 0f));

            var inventory = new InventoryComponent(new InventoryObject());
            e.AddComponent(inventory);
            AddTestItems(inventory);

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

            // Dev test
            component.EquipItem(EquipmentSlotType.Chest, _itemCatalogue.CreateItem("Camisa"));
            component.EquipItem(EquipmentSlotType.Chest, _itemCatalogue.CreateItem("Pechera"));
            

            return component;
        }

        #region Dev Testing

        /// <summary>
        /// Dev-only: seeds the inventory with items of assorted dimensions so the
        /// tetris grid rendering can be checked (1x3 blade, 1x1 food, containers...).
        /// Remove once items can be picked up in-game.
        /// </summary>
        private void AddTestItems(InventoryComponent inventory)
        {
            InventoryObject inv = inventory.Inventory;

            AddTestItem(inv, "Espada de hierro", 1);
            AddTestItem(inv, "Arco corto", 1);
            AddTestItem(inv, "Manzana", 5);
            AddTestItem(inv, "Venda", 3);
            AddTestItem(inv, "Odre", 1);
        }

        private void AddTestItem(InventoryObject inv, string itemName, int amount)
        {
            int remaining = inv.AddItem(_itemCatalogue.CreateItem(itemName), amount);
            if (remaining > 0)
                CoreLogger.Instance.LogWarning(
                    $"PrototypeFactory: no room for {remaining}x '{itemName}' in the test inventory.");
        }

        #endregion
    }
}
