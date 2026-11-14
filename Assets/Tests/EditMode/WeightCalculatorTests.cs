using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.Inventory;
using Core.Observer;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// H. Peso de cualquier entidad (Fase 1.5, M2): una sola respuesta para el inventario y
    /// para la fisica, y el aviso de que puede haber cambiado.
    /// </summary>
    public class WeightCalculatorTests
    {
        private WeightLog _log;

        [SetUp]
        public void Montar()
        {
            new ServiceRig();   // logger mudo y bus limpio
            _log = new WeightLog();
            EventBus.GetInstance().Subscribe(GameEventType.WeightChanged, _log);
        }

        [Test]
        public void Un_cuerpo_pesa_lo_suyo_mas_lo_que_lleva()
        {
            (InGameEntity body, InventoryObject inventory) = TestItems.Body();   // 75 kg
            inventory.AddItem(TestItems.Item(TestItems.SWORD, weight: 2f, maxStack: 10), 3);

            Assert.AreEqual(81f, WeightCalculator.Of(body), 0.001f);
        }

        [Test]
        public void Un_cuerpo_cuenta_lo_que_va_en_la_mochila_puesta()
        {
            (InGameEntity body, _) = TestItems.Body();
            ItemEntity backpack = TestItems.Container(3, 3, maxWeight: 100000f);
            Assert.IsTrue(WornContainerBinder.Attach(body, backpack));
            TestItems.InventoryOf(backpack).AddItem(TestItems.Item(TestItems.SWORD, weight: 10f, maxStack: 1), 1);

            // 75 del cuerpo, la mochila (que pesa aunque este vacia) y los 10 de dentro
            Assert.AreEqual(75f + WeightCalculator.Of(backpack), WeightCalculator.Of(body), 0.001f);
            Assert.Greater(WeightCalculator.Of(backpack), 10f);
        }

        [Test]
        public void Un_monton_pesa_todos_sus_lotes()
        {
            InGameEntity pile = new InGameEntity(IdGenerator.GenerateNewId());
            pile.AddComponent(new GroundLotComponent(new List<SubLot>
            {
                new SubLot(TestItems.Apple(weight: 0.5f), 4),
                new SubLot(TestItems.Apple(weight: 0.5f, durability: 50f), 2),
            }));

            Assert.AreEqual(3f, WeightCalculator.Of(pile), 0.001f);
        }

        [Test]
        public void Un_contenedor_visto_como_entidad_cuenta_su_contenido()
        {
            ItemEntity chest = TestItems.Container(4, 4, maxWeight: 100000f);
            TestItems.InventoryOf(chest).AddItem(TestItems.Item(TestItems.SWORD, weight: 7f, maxStack: 1), 1);
            IEntity asEntity = chest;   // la sobrecarga la decide el tipo de la variable

            Assert.AreEqual(WeightCalculator.Of(chest), WeightCalculator.Of(asEntity), 0.001f);
            Assert.Greater(WeightCalculator.Of(asEntity), 7f, "el arcon suma su contenido");
        }

        [Test]
        public void Lo_que_no_pesa_nada_conocido_da_cero()
        {
            Assert.AreEqual(0f, WeightCalculator.Of(new InGameEntity(IdGenerator.GenerateNewId())));
            Assert.AreEqual(0f, WeightCalculator.Of((IEntity)null));
        }

        [Test]
        public void Cambiar_la_mochila_puesta_avisa_del_peso_a_la_mochila_y_al_portador()
        {
            (InGameEntity body, _) = TestItems.Body();
            body.AddComponent(new MovementComponent(2f));
            ItemEntity backpack = TestItems.Container(3, 3, maxWeight: 100000f);
            Assert.IsTrue(WornContainerBinder.Attach(body, backpack));
            TestItems.InventoryOf(backpack).AddItem(TestItems.Item(TestItems.SWORD, weight: 1f, maxStack: 1), 1);

            new InventorySystem().EvaluateAndFireEvents(backpack, false);

            CollectionAssert.Contains(_log.Entities, backpack);
            CollectionAssert.Contains(_log.Entities, body, "el peso del portador tambien cambia");
        }

        /// <summary>Apunta a quien va cada aviso de peso.</summary>
        private sealed class WeightLog : IEventObserver
        {
            public readonly List<IEntity> Entities = new List<IEntity>();
            public void UpdateOnEvent(GameEvent gameEvent) => Entities.Add(gameEvent.GetEntity());
        }
    }
}
