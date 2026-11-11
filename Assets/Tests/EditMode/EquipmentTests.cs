using System.Collections.Generic;
using System.Linq;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Core.Inventory;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// F. Equipo: capa exterior, categoria repetida, ocupacion completa y desequipar sin sitio.
    /// </summary>
    public class EquipmentTests
    {
        private static EquipmentComponent Equipment(params EquipmentSlotType[] slots)
        {
            EquipmentComponent eq = new EquipmentComponent(new List<EquipmentSlotType>(slots));
            foreach (EquipmentSlotType s in slots) eq.AddSlot(s, maxLayers: 3);
            return eq;
        }

        private static ItemEntity Shirt() => TestItems.Garment(GarmentCategory.Shirt, false, 1f, false, EquipmentSlotType.Chest);
        private static ItemEntity Plate() => TestItems.Garment(GarmentCategory.Plate, true, 5f, false, EquipmentSlotType.Chest);
        private static ItemEntity Robe() => TestItems.Garment(GarmentCategory.Robe, true, 2f, false, EquipmentSlotType.Chest);

        [Test]
        public void Con_capa_exterior_puesta_otra_exterior_se_rechaza_y_una_interior_se_cuela_debajo()
        {
            EquipmentComponent eq = Equipment(EquipmentSlotType.Chest);
            ItemEntity plate = Plate();
            ItemEntity shirt = Shirt();
            eq.EquipItem(EquipmentSlotType.Chest, plate);

            Assert.AreEqual(EquipResult.TopLayerBlocked, eq.EquipItem(EquipmentSlotType.Chest, Robe()));
            Assert.AreEqual(EquipResult.SuccessEquip, eq.EquipItem(EquipmentSlotType.Chest, shirt));

            EquipmentSlot chest = eq.GetEquipmentSlot(EquipmentSlotType.Chest);
            Assert.AreSame(plate, chest.GetTopItem());
            Assert.AreSame(shirt, chest.Items[0]);
        }

        [Test]
        public void No_se_pueden_llevar_dos_prendas_de_la_misma_categoria_en_un_slot()
        {
            EquipmentComponent eq = Equipment(EquipmentSlotType.Chest);
            eq.EquipItem(EquipmentSlotType.Chest, Shirt());

            Assert.AreEqual(EquipResult.DuplicateCategory, eq.EquipItem(EquipmentSlotType.Chest, Shirt()));
        }

        [Test]
        public void Una_prenda_de_ocupacion_completa_ocupa_todos_sus_slots()
        {
            EquipmentComponent eq = Equipment(EquipmentSlotType.LeftHand, EquipmentSlotType.RightHand);
            EquipmentSlotType[] hands = { EquipmentSlotType.LeftHand, EquipmentSlotType.RightHand };
            ItemEntity bow = TestItems.Garment(GarmentCategory.Glove, false, 1f, true, hands);

            Assert.AreEqual(EquipResult.SuccessEquip, eq.EquipItem(hands, bow, fullOcupancy: true));

            Assert.Contains(bow, eq.GetEquipmentSlot(EquipmentSlotType.LeftHand).Items);
            Assert.Contains(bow, eq.GetEquipmentSlot(EquipmentSlotType.RightHand).Items);
        }

        [Test]
        public void Una_prenda_de_ocupacion_completa_cuenta_una_sola_vez()
        {
            EquipmentComponent eq = Equipment(EquipmentSlotType.LeftHand, EquipmentSlotType.RightHand);
            EquipmentSlotType[] hands = { EquipmentSlotType.LeftHand, EquipmentSlotType.RightHand };
            eq.EquipItem(hands, TestItems.Garment(GarmentCategory.Glove, false, 1f, true, hands), fullOcupancy: true);

            Assert.AreEqual(1, eq.EquippedItems().Count());
        }

        [Test]
        public void Desequipar_sin_sitio_deja_la_prenda_puesta()
        {
            ServiceRig rig = new ServiceRig();

            // Portador sin cuerpo: su techo de peso sale del StorageComponent (0,5 kg).
            InGameEntity wearer = new InGameEntity(990100);
            wearer.AddComponent(new StorageComponent(5, 7, 0.5f));
            wearer.AddComponent(new InventoryComponent(new InventoryObject(wearer)));
            EquipmentComponent eq = Equipment(EquipmentSlotType.Chest);
            wearer.AddComponent(eq);
            ItemEntity shirt = Shirt();   // 1 kg: no cabe por peso
            eq.EquipItem(EquipmentSlotType.Chest, shirt);

            int moved = rig.Service.TryUnequipItem(wearer, shirt, new List<EquipmentSlotType> { EquipmentSlotType.Chest });

            Assert.AreEqual(0, moved);
            Assert.IsTrue(eq.HasEquiped(shirt));
            Assert.IsEmpty(wearer.GetComponent<InventoryComponent>().Inventory.GetChildren());
        }
    }
}
