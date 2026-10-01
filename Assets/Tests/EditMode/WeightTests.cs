using Core.ECS.Entity;
using Core.Inventory;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// D. Peso: techo propio, techo en paralelo con quien lo contiene, mover dentro del mismo
    /// portador y peso de un contenedor con su contenido.
    /// </summary>
    public class WeightTests
    {
        [Test]
        public void Una_bolsa_llena_por_peso_rechaza_el_resto_aunque_le_sobren_celdas()
        {
            InventoryObject bag = TestItems.InventoryOf(TestItems.Container(4, 4, maxWeight: 2f));
            ItemEntity apple = TestItems.Apple(weight: 0.5f);

            Assert.AreEqual(4, bag.FitByWeight(apple, 10));           // 2 kg / 0,5
            Assert.AreEqual(16, bag.GetGrid().GetFreeCellCount());    // sitio sobra
        }

        [Test]
        public void El_peso_es_paralelo_manda_el_techo_mas_bajo_de_la_cadena()
        {
            ItemEntity chestItem = TestItems.Container(4, 4, maxWeight: 3f);
            InventoryObject chest = TestItems.InventoryOf(chestItem);
            ItemEntity backpackItem = TestItems.Container(3, 3, maxWeight: 100f, ownWeight: 1f);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);
            chest.AddItem(backpackItem, 1);                            // al arcon le quedan 2 kg

            ItemEntity apple = TestItems.Apple(weight: 0.5f);

            Assert.AreEqual(4, backpack.FitByWeight(apple, 10));       // la mochila admitiria 200
            Assert.IsTrue(backpack.CarrierBlocks(2.5f));               // y el culpable es el arcon
        }

        [Test]
        public void Mover_dentro_del_mismo_portador_no_cuenta_contra_su_techo()
        {
            ItemEntity chestItem = TestItems.Container(4, 4, maxWeight: 5f);
            InventoryObject chest = TestItems.InventoryOf(chestItem);
            ItemEntity backpackItem = TestItems.Container(3, 3, maxWeight: 100f, ownWeight: 1f);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);
            chest.AddItem(backpackItem, 1);
            ItemEntity apple = TestItems.Apple(weight: 0.5f);
            backpack.StackOnto(apple, 8);                              // arcon: 1 + 4 = 5 kg, lleno

            Assert.AreEqual(0, chest.FitByWeight(apple, 1), "desde fuera no cabe");
            Assert.AreEqual(1, chest.FitByWeight(apple, 1, source: backpack), "desde su propia mochila si");
        }

        [Test]
        public void Un_contenedor_pesa_lo_suyo_mas_su_contenido()
        {
            ItemEntity backpackItem = TestItems.Container(3, 3, maxWeight: 100f, ownWeight: 1f);
            TestItems.InventoryOf(backpackItem).StackOnto(TestItems.Apple(weight: 0.5f), 8);

            Assert.AreEqual(5f, ItemWeight.Of(backpackItem), 1e-4f);
        }
    }
}
