using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Core.Inventory;

namespace Core.Tests
{
    /// <summary>
    /// Fabrica de objetos de prueba: items y contenedores montados a mano, sin catalogo ni
    /// JSON. Cada test pide exactamente lo que necesita (tamano, peso, pila maxima) y no
    /// depende de que el catalogo del juego cambie.
    /// </summary>
    internal static class TestItems
    {
        /* Ids altos para no chocar con nada que el juego genere. */
        private static int _nextEntityId = 900000;

        public const int APPLE = 1;
        public const int BANDAGE = 2;
        public const int SWORD = 3;
        public const int CONTAINER = 50;

        /// <summary>Un item sin mas: tipo, peso por unidad, pila maxima y tamano en celdas.</summary>
        public static ItemEntity Item(int typeId, float weight = 1f, int maxStack = 10,
                                      int w = 1, int h = 1, float durability = 100f)
        {
            ItemEntity item = new ItemEntity(_nextEntityId++);
            item.AddComponent(new BaseItemComponent(typeId, "item" + typeId, weight, maxStack, w, h, durability));
            return item;
        }

        public static ItemEntity Apple(float weight = 0.5f, float durability = 100f)
            => Item(APPLE, weight, maxStack: 10, durability: durability);

        public static ItemEntity Bandage() => Item(BANDAGE, 0.1f, maxStack: 5);

        /// <summary>
        /// Un contenedor (arcon, mochila): item de pila 1 con rejilla, techo de peso e
        /// inventario propio. El StorageComponent va antes que el inventario porque este
        /// lee de el el tamano de la rejilla.
        /// </summary>
        public static ItemEntity Container(int gridH, int gridW, float maxWeight,
                                           float ownWeight = 1f, int w = 2, int h = 2)
        {
            ItemEntity c = new ItemEntity(_nextEntityId++);
            c.AddComponent(new BaseItemComponent(CONTAINER, "contenedor", ownWeight, 1, w, h));
            c.AddComponent(new StorageComponent(gridH, gridW, maxWeight));
            c.AddComponent(new InventoryComponent(new InventoryObject(c)));
            return c;
        }

        public const int GARMENT = 60;

        /// <summary>Una prenda: a que slots va, si es capa exterior, su categoria y si ocupa todos sus slots a la vez.</summary>
        public static ItemEntity Garment(GarmentCategory category, bool topLayer, float weight = 1f,
                                         bool fullOccupancy = false, params EquipmentSlotType[] slots)
        {
            ItemEntity item = Item(GARMENT + (int)category, weight, maxStack: 1);
            item.AddComponent(new WearableComponent(new List<EquipmentSlotType>(slots), topLayer, category, fullOccupancy));
            return item;
        }

        /// <summary>
        /// Un cuerpo adulto con su inventario: lo unico cuyo techo de peso no frena, sino que
        /// sobrecarga. Sin hambre ni cansancio, para que su techo dependa solo del cuerpo.
        /// </summary>
        public static (InGameEntity body, InventoryObject inventory) Body()
        {
            InGameEntity body = new InGameEntity(_nextEntityId++);
            body.AddComponent(new BodyComponent(height: 1.75f, weight: 75f, age: 25f, sex: 0));
            InventoryObject inventory = new InventoryObject(body);
            body.AddComponent(new InventoryComponent(inventory));
            return (body, inventory);
        }

        public static InventoryObject InventoryOf(ItemEntity container)
            => container.GetComponent<InventoryComponent>().Inventory;
    }
}
