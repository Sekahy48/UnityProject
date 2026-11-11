using System.Collections.Generic;
using System.Linq;
using Core.ECS.Entity;
using Core.Inventory;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// C. El inventario (InventoryObject): apilar y abrir pilas, rejilla llena, colocar en celda,
    /// contenedores guardados y puestos, ciclos, sacar nodos y clonar.
    /// </summary>
    public class InventoryObjectTests
    {
        private static List<ItemObject> Leaves(InventoryObject inv)
            => inv.GetChildren().OfType<ItemObject>().ToList();

        [Test]
        public void Meter_mas_de_una_pila_la_completa_y_abre_otra_en_celdas_libres()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            ItemEntity apple = TestItems.Apple();   // pila maxima 10

            int left = chest.StackOnto(apple, 15);

            Assert.AreEqual(0, left);
            List<ItemObject> stacks = Leaves(chest);
            Assert.AreEqual(2, stacks.Count);
            CollectionAssert.AreEquivalent(new[] { 10, 5 }, stacks.Select(s => s.GetAmount()));
            Assert.AreEqual(2, chest.GetGrid().GetElements().Count);
        }

        [Test]
        public void Con_la_rejilla_llena_devuelve_lo_que_no_entra()
        {
            InventoryObject tiny = TestItems.InventoryOf(TestItems.Container(1, 2, 1000f));   // 2 celdas

            int left = tiny.StackOnto(TestItems.Apple(), 25);   // 2 pilas de 10

            Assert.AreEqual(5, left);
            Assert.AreEqual(0, tiny.GetGrid().GetFreeCellCount());
        }

        [Test]
        public void Colocar_sobre_el_mismo_tipo_apila_y_sobre_otro_tipo_se_rechaza()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            ItemEntity apple = TestItems.Apple();
            chest.AddItemAt(apple, 3, new GridPos(0, 0));

            int leftSame = chest.AddItemAt(apple, 4, new GridPos(0, 0));
            int leftOther = chest.AddItemAt(TestItems.Bandage(), 2, new GridPos(0, 0));

            Assert.AreEqual(0, leftSame);
            Assert.AreEqual(2, leftOther);
            Assert.AreEqual(1, Leaves(chest).Count);
            Assert.AreEqual(7, Leaves(chest)[0].GetAmount());
        }

        [Test]
        public void Un_contenedor_guardado_ocupa_celdas_y_cuelga_del_que_lo_guarda()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            ItemEntity backpackItem = TestItems.Container(3, 3, 100f, w: 2, h: 2);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);

            int left = chest.AddItem(backpackItem, 1);

            Assert.AreEqual(0, left);
            Assert.AreSame(chest, backpack.Parent);
            Assert.IsNotNull(chest.GetGrid().GetElementOf(backpack.GetNodeId()));
            Assert.AreEqual(16 - 4, chest.GetGrid().GetFreeCellCount());
        }

        [Test]
        public void Un_contenedor_puesto_cuelga_del_portador_sin_ocupar_celdas()
        {
            InventoryObject wearer = new InventoryObject(new InGameEntity(990001));
            int freeBefore = wearer.GetGrid().GetFreeCellCount();
            InventoryObject backpack = TestItems.InventoryOf(TestItems.Container(3, 3, 100f));

            wearer.AttachWornContainer(backpack);

            Assert.AreSame(wearer, backpack.Parent);
            Assert.Contains(backpack, wearer.GetChildren());
            Assert.AreEqual(freeBefore, wearer.GetGrid().GetFreeCellCount());
        }

        [Test]
        public void Un_contenedor_no_puede_guardarse_en_si_mismo_ni_en_algo_que_contiene()
        {
            ItemEntity chestItem = TestItems.Container(4, 4, 1000f);
            InventoryObject chest = TestItems.InventoryOf(chestItem);
            ItemEntity backpackItem = TestItems.Container(3, 3, 100f);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);
            chest.AddItem(backpackItem, 1);   // la mochila esta dentro del arcon

            Assert.AreEqual(1, chest.AddItem(chestItem, 1), "en si mismo");
            Assert.AreEqual(1, backpack.AddItem(chestItem, 1), "dentro de lo que contiene");
            Assert.IsNull(chest.Parent);
        }

        [Test]
        public void Sacar_un_nodo_libera_sus_celdas_y_no_mueve_al_resto()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            chest.AddItemAt(TestItems.Apple(), 3, new GridPos(0, 0));
            chest.AddItemAt(TestItems.Bandage(), 2, new GridPos(0, 2));
            ItemObject apples = Leaves(chest).First(n => n.GetTypeId() == TestItems.APPLE);
            ItemObject bandages = Leaves(chest).First(n => n.GetTypeId() == TestItems.BANDAGE);

            Assert.IsTrue(chest.CleanNode(apples));

            Assert.AreEqual(-1, chest.GetGrid().GetCellAt(new GridPos(0, 0)));
            Assert.AreEqual(new GridPos(0, 2), chest.GetGrid().GetElementOf(bandages.GetNodeId()).GetPos());
        }

        [Test]
        public void Sacar_un_contenedor_guardado_le_quita_el_padre()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            ItemEntity backpackItem = TestItems.Container(3, 3, 100f);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);
            chest.AddItem(backpackItem, 1);

            Assert.IsTrue(chest.CleanNode(backpack));

            Assert.IsNull(backpack.Parent);
            Assert.AreEqual(16, chest.GetGrid().GetFreeCellCount());
        }

        [Test]
        public void Clonar_conserva_las_posiciones_y_deja_el_clon_sin_padre()
        {
            InventoryObject chest = TestItems.InventoryOf(TestItems.Container(4, 4, 1000f));
            ItemEntity backpackItem = TestItems.Container(3, 3, 100f);
            InventoryObject backpack = TestItems.InventoryOf(backpackItem);
            chest.AddItem(backpackItem, 1);
            backpack.AddItemAt(TestItems.Apple(), 4, new GridPos(1, 2));

            InventoryObject clone = (InventoryObject)backpack.Clone();

            Assert.IsNull(clone.Parent);
            List<ItemObject> cloned = Leaves(clone);
            Assert.AreEqual(1, cloned.Count);
            Assert.AreEqual(4, cloned[0].GetAmount());
            Assert.AreEqual(new GridPos(1, 2), clone.GetGrid().GetElementOf(cloned[0].GetNodeId()).GetPos());
        }

        [Test]
        public void Apilar_rellena_una_pila_que_no_es_la_primera_de_su_tipo()
        {
            InventoryObject tiny = TestItems.InventoryOf(TestItems.Container(1, 2, 1000f));   // 2 celdas
            tiny.StackOntoHere(TestItems.Apple(), 10);   // primera pila, llena
            tiny.StackOntoHere(TestItems.Apple(), 5);    // segunda pila a medias; rejilla llena

            int left = tiny.StackOntoHere(TestItems.Apple(), 3);

            Assert.AreEqual(0, left, "la primera pila esta llena, pero la segunda tiene hueco");
            CollectionAssert.AreEquivalent(new[] { 10, 8 }, Leaves(tiny).Select(s => s.GetAmount()));
        }

        [Test]
        public void Apilar_prefiere_la_pila_que_ya_tiene_esa_variante()
        {
            InventoryObject tiny = TestItems.InventoryOf(TestItems.Container(1, 2, 1000f));
            // AddItem y no StackOntoHere para montar las dos pilas: apilando, la segunda tanda
            // se meteria en la primera (que tiene hueco) y no habria dos pilas que comparar.
            tiny.AddItem(TestItems.Apple(durability: 50f), 5);    // primera en la lista
            tiny.AddItem(TestItems.Apple(durability: 100f), 5);   // segunda: la variante buena

            ItemEntity fresh = TestItems.Apple(durability: 100f);
            tiny.StackOntoHere(fresh, 3);

            ItemObject withFresh = Leaves(tiny).Single(s => s.GetBatch().HasVariant(fresh));
            Assert.AreEqual(8, withFresh.GetAmount(), "va con sus iguales aunque haya otra pila antes");
            Assert.AreEqual(2, Leaves(tiny).Count);
        }
    }
}
