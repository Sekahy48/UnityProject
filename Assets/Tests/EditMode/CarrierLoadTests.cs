using System.Collections.Generic;
using System.Linq;
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
    /// F. La banda de carga es del portador. Un contenedor dentro de otro inventario (mochila
    /// puesta, bolsa dentro de la mochila) no tiene cuerpo: al cambiar su contenido, quien
    /// tiene que enterarse es quien lo lleva, que es a quien escuchan el movimiento y el HUD.
    /// </summary>
    public class CarrierLoadTests
    {
        private static readonly GameEventType[] _loadBands =
        {
            GameEventType.NormalWeight, GameEventType.ExtraWeight,
            GameEventType.Overweight, GameEventType.Immobile,
        };

        private InventorySystem _system;
        private BandRecorder _bands;

        [SetUp]
        public void Montar()
        {
            new ServiceRig();   // solo por lo que limpia: suscriptores del bus y logger mudo
            _system = new InventorySystem();
            _bands = new BandRecorder();
            foreach (GameEventType band in _loadBands)
                EventBus.GetInstance().Subscribe(band, _bands);
        }

        /// <summary>Lo que necesita la rama del cuerpo: cuerpo, inventario y movimiento.</summary>
        private static InGameEntity Carrier()
        {
            InGameEntity body = TestItems.Body().body;
            body.AddComponent(new MovementComponent(2f));
            return body;
        }

        /// <summary>Una mochila puesta en el portador, sin techo propio que estorbe.</summary>
        private static ItemEntity WornBackpack(IEntity carrier)
        {
            ItemEntity backpack = TestItems.Container(3, 3, maxWeight: 100000f);
            Assert.IsTrue(WornContainers.Attach(carrier, backpack), "la mochila tiene que colgar del portador");
            return backpack;
        }

        private static ItemEntity Anvil() => TestItems.Item(TestItems.SWORD, weight: 500f, maxStack: 10);

        [Test]
        public void Meter_peso_en_la_mochila_puesta_avisa_al_portador()
        {
            InGameEntity carrier = Carrier();
            ItemEntity backpack = WornBackpack(carrier);
            TestItems.InventoryOf(backpack).AddItem(Anvil(), 1);

            _system.EvaluateAndFireEvents(backpack, false);

            Assert.AreEqual(1, _bands.For(carrier).Count, "una banda para el portador");
            Assert.AreEqual(GameEventType.Immobile, _bands.For(carrier).Single());
            Assert.AreEqual(0, _bands.For(backpack).Count, "la mochila no tiene banda propia");
        }

        [Test]
        public void Una_bolsa_dentro_de_la_mochila_avisa_al_portador_de_arriba_del_todo()
        {
            InGameEntity carrier = Carrier();
            ItemEntity backpack = WornBackpack(carrier);
            ItemEntity bag = TestItems.Container(2, 2, maxWeight: 100000f, w: 1, h: 1);
            TestItems.InventoryOf(backpack).AddItem(bag, 1);
            TestItems.InventoryOf(bag).AddItem(Anvil(), 1);

            _system.EvaluateAndFireEvents(bag, false);

            Assert.AreEqual(GameEventType.Immobile, _bands.For(carrier).Single());
        }

        [Test]
        public void Un_arcon_suelto_no_sube_a_nadie_ni_entra_en_bucle()
        {
            ItemEntity chest = TestItems.Container(4, 4, maxWeight: 100000f);
            TestItems.InventoryOf(chest).AddItem(Anvil(), 1);

            Assert.DoesNotThrow(() => _system.EvaluateAndFireEvents(chest, false));
            Assert.AreEqual(0, _bands.All.Count);
        }

        /// <summary>
        /// Como en el juego: el jugador no se construye, se CLONA de un prototipo
        /// (EntityManager.CreateEntity / GetPlayer). Si el clon de su inventario siguiera
        /// apuntando al prototipo como dueno, la banda saldria a nombre del prototipo y el
        /// movimiento y el HUD, que filtran por el jugador, no se enterarian.
        /// </summary>
        [Test]
        public void Un_portador_clonado_de_un_prototipo_recibe_la_banda_a_su_nombre()
        {
            IEntity prototype = Carrier();
            IEntity carrier = prototype.Clone();
            ItemEntity backpack = WornBackpack(carrier);
            TestItems.InventoryOf(backpack).AddItem(Anvil(), 1);

            _system.EvaluateAndFireEvents(backpack, false);

            Assert.AreSame(carrier, carrier.GetComponent<InventoryComponent>().Inventory.GetHolder(),
                           "el inventario del clon tiene que ser del clon, no del prototipo");
            Assert.AreEqual(GameEventType.Immobile, _bands.For(carrier).SingleOrDefault(),
                            "la banda tiene que ir a nombre del clon");
            Assert.AreEqual(0, _bands.For(prototype).Count, "y no a nombre del prototipo");
        }

        /// <summary>Apunta cada banda publicada con la entidad a la que va.</summary>
        private sealed class BandRecorder : IEventObserver
        {
            public readonly List<(GameEventType band, IEntity entity)> All = new();

            public void UpdateOnEvent(GameEvent gameEvent)
                => All.Add((gameEvent.GetEventType(), gameEvent.GetEntity()));

            public List<GameEventType> For(IEntity entity)
                => All.Where(e => ReferenceEquals(e.entity, entity)).Select(e => e.band).ToList();
        }
    }
}
