using System.Linq;
using Core.ECS.Entity;
using Core.Inventory;
using Core.Services;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// E. Mano y transferencias (InventoryService): la mano es una reserva, cancelar es gratis,
    /// colocar en parte, deshacer lo que no entra y paridad entre veredicto y colocacion.
    /// </summary>
    public class HandAndTransferTests
    {
        private ServiceRig _rig;
        private InventoryService Service => _rig.Service;

        [SetUp]
        public void Montar() => _rig = new ServiceRig();

        /// <summary>Un arcon con 5 manzanas en (0,0). Devuelve el arcon, su inventario y el nodo.</summary>
        private static (ItemEntity chest, InventoryObject inv, ItemObject apples) ChestWithApples(float maxWeight = 1000f)
        {
            ItemEntity chest = TestItems.Container(4, 4, maxWeight);
            InventoryObject inv = TestItems.InventoryOf(chest);
            inv.AddItemAt(TestItems.Apple(weight: 0.5f), 5, new GridPos(0, 0));
            return (chest, inv, inv.GetChildren().OfType<ItemObject>().Single());
        }

        private void GrabFrom(ItemEntity owner, InventoryObject inv, ItemObject node, int amount)
            => Service.GrabFrom(Service.NodeOrigin(owner, inv, node), amount);

        private static int UnitsIn(InventoryObject inv)
            => inv.GetChildren().OfType<ItemObject>().Sum(n => n.GetAmount());

        [Test]
        public void Agarrar_no_mueve_nada_las_unidades_siguen_en_su_nodo()
        {
            var (chest, inv, apples) = ChestWithApples();

            GrabFrom(chest, inv, apples, 3);

            Assert.IsTrue(Service.IsHandCarrying());
            Assert.AreEqual(3, Service.GetGrabbedAmount());
            Assert.AreEqual(5, apples.GetAmount());
        }

        [Test]
        public void Cancelar_la_mano_deja_todo_como_estaba()
        {
            var (chest, inv, apples) = ChestWithApples();
            GrabFrom(chest, inv, apples, 3);

            Service.EmptyHand();

            Assert.IsFalse(Service.IsHandCarrying());
            Assert.AreEqual(5, apples.GetAmount());
            Assert.AreEqual(new GridPos(0, 0), inv.GetGrid().GetElementOf(apples.GetNodeId()).GetPos());
        }

        [Test]
        public void Colocar_parte_de_lo_que_se_lleva_deja_el_resto_en_la_mano()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity small = TestItems.Container(4, 4, maxWeight: 1f);   // admite 2 manzanas
            GrabFrom(chest, inv, apples, 5);

            int left = Service.PlaceAmountFromHand(small, new GridPos(0, 0));

            Assert.AreEqual(3, left);
            Assert.AreEqual(3, Service.GetGrabbedAmount());
            Assert.AreEqual(2, UnitsIn(TestItems.InventoryOf(small)));
            Assert.AreEqual(3, UnitsIn(inv));
        }

        [Test]
        public void Lo_que_no_entra_en_una_transferencia_vuelve_a_su_origen()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity small = TestItems.Container(4, 4, maxWeight: 1f);   // admite 2 manzanas

            int moved = Service.TryMoveItemTo(Service.NodeOrigin(chest, inv, apples), null, 5, small, new GridPos(0, 0));

            Assert.AreEqual(2, moved);
            Assert.AreEqual(3, UnitsIn(inv));
            Assert.AreEqual(2, UnitsIn(TestItems.InventoryOf(small)));
        }

        [Test]
        public void Si_no_entra_nada_el_origen_queda_intacto()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity other = TestItems.Container(4, 4, 1000f);
            TestItems.InventoryOf(other).AddItemAt(TestItems.Bandage(), 1, new GridPos(0, 0));   // celda ocupada por otro tipo

            int moved = Service.TryMoveItemTo(Service.NodeOrigin(chest, inv, apples), null, 5, other, new GridPos(0, 0));

            Assert.AreEqual(0, moved);
            Assert.AreEqual(5, apples.GetAmount());
            Assert.AreEqual(new GridPos(0, 0), inv.GetGrid().GetElementOf(apples.GetNodeId()).GetPos());
        }

        [Test]
        public void Paridad_si_el_veredicto_es_cabe_colocar_mueve_todo()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity big = TestItems.Container(4, 4, 1000f);
            GrabFrom(chest, inv, apples, 5);

            Assert.AreEqual(PlacementVerdict.Fits, Service.EvaluatePlacement(big, new GridPos(1, 1)));
            Assert.AreEqual(0, Service.PlaceAmountFromHand(big, new GridPos(1, 1)));
            Assert.AreEqual(5, UnitsIn(TestItems.InventoryOf(big)));
        }

        [Test]
        public void Paridad_si_el_veredicto_es_parcial_colocar_deja_resto()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity small = TestItems.Container(4, 4, maxWeight: 1f);
            GrabFrom(chest, inv, apples, 5);

            Assert.AreEqual(PlacementVerdict.Partial, Service.EvaluatePlacement(small, new GridPos(0, 0)));
            Assert.AreEqual(3, Service.PlaceAmountFromHand(small, new GridPos(0, 0)));
        }

        [Test]
        public void Paridad_si_el_veredicto_es_bloqueado_colocar_no_mueve_nada()
        {
            var (chest, inv, apples) = ChestWithApples();
            ItemEntity other = TestItems.Container(4, 4, 1000f);
            TestItems.InventoryOf(other).AddItemAt(TestItems.Bandage(), 1, new GridPos(0, 0));
            GrabFrom(chest, inv, apples, 5);

            Assert.AreEqual(PlacementVerdict.Blocked, Service.EvaluatePlacement(other, new GridPos(0, 0)));
            Assert.AreEqual(5, Service.PlaceAmountFromHand(other, new GridPos(0, 0)));
            Assert.AreEqual(5, apples.GetAmount());
        }

        [Test]
        public void Paridad_si_el_veredicto_es_intercambio_los_dos_cambian_de_celdas()
        {
            var (chest, inv, apples) = ChestWithApples();
            inv.AddItemAt(TestItems.Bandage(), 1, new GridPos(0, 2));
            ItemObject bandage = inv.GetChildren().OfType<ItemObject>().Single(n => n.GetTypeId() == TestItems.BANDAGE);
            GrabFrom(chest, inv, apples, 5);   // el nodo entero

            Assert.AreEqual(PlacementVerdict.Swap, Service.EvaluatePlacement(chest, new GridPos(0, 2)));
            Assert.IsTrue(Service.SwapFromHand(chest, new GridPos(0, 2)));
            Assert.AreEqual(new GridPos(0, 2), inv.GetGrid().GetElementOf(apples.GetNodeId()).GetPos());
            Assert.AreEqual(new GridPos(0, 0), inv.GetGrid().GetElementOf(bandage.GetNodeId()).GetPos());
        }
    }
}
