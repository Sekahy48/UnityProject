using System.Collections.Generic;
using Core.ECS.Entity;
using Core.Inventory;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>B. Pilas y sub-lotes (BatchItem): agrupar por equivalencia, tope de pila, consumo y peso.</summary>
    public class BatchItemTests
    {
        private static int AmountOf(BatchItem batch, ItemEntity variant)
        {
            foreach (SubLot lot in batch.GetSubLots())
                if (lot.Item.Equivalent(variant)) return lot.Amount;
            return 0;
        }

        [Test]
        public void Un_item_equivalente_suma_al_mismo_sublote()
        {
            ItemEntity apple = TestItems.Apple();
            BatchItem batch = new BatchItem(apple, 3);

            int left = batch.AddAmount(apple.Clone(), 2);

            Assert.AreEqual(0, left);
            Assert.AreEqual(1, batch.GetSubLots().Count);
            Assert.AreEqual(5, batch.GetTotalAmount());
        }

        [Test]
        public void Un_item_con_otro_estado_abre_un_sublote_nuevo_en_la_misma_pila()
        {
            BatchItem batch = new BatchItem(TestItems.Apple(), 3);

            batch.AddAmount(TestItems.Apple(durability: 50f), 2);

            Assert.AreEqual(2, batch.GetSubLots().Count);
            Assert.AreEqual(5, batch.GetTotalAmount());
        }

        [Test]
        public void Pasar_del_tope_de_pila_devuelve_el_sobrante()
        {
            ItemEntity apple = TestItems.Apple();   // pila maxima 10
            BatchItem batch = new BatchItem(apple, 8);

            int left = batch.AddAmount(apple, 5);

            Assert.AreEqual(3, left);
            Assert.AreEqual(10, batch.GetTotalAmount());
        }

        [Test]
        public void Consumir_una_variante_saca_solo_de_su_sublote()
        {
            ItemEntity fresh = TestItems.Apple();
            ItemEntity worn = TestItems.Apple(durability: 50f);
            BatchItem batch = new BatchItem(fresh, 3);
            batch.AddAmount(worn, 2);

            int consumed = batch.ConsumeAmount(worn, 1);

            Assert.AreEqual(1, consumed);
            Assert.AreEqual(3, AmountOf(batch, fresh));
            Assert.AreEqual(1, AmountOf(batch, worn));
        }

        [Test]
        public void El_peso_de_la_pila_es_la_suma_de_cada_sublote()
        {
            BatchItem batch = new BatchItem(TestItems.Apple(weight: 0.2f), 3);   // 0,6
            batch.AddAmount(TestItems.Apple(weight: 0.3f), 2);                   // 0,6

            Assert.AreEqual(1.2f, batch.GetTotalWeight(), 1e-4f);
        }
    }
}
