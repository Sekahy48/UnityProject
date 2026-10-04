using System.Collections.Generic;
using System.Linq;
using Core.Contexts;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.Factories;
using Core.Inventory;
using Core.Item;
using Core.MVC.Presenter;
using Core.Observer;
using Core.Services;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// G. Mundo: cierre por distancia con margen, motivo al recoger y tirar suelto o en monton.
    /// </summary>
    public class WorldTests
    {
        private EntityManager _entities;
        private WorldInteractionSystem _world;
        private WorldInteractionService _service;
        private EventLog _events;

        /// <summary>Linker que no hace nada: en los tests no hay GameObjects que enlazar.</summary>
        private sealed class NullLinker : IEntityLinker
        {
            public void Link(IEntity entity, EntityType entityType) { }
            public void Unlink(IEntity entity) { }
        }

        /// <summary>Apunta los eventos que se publican, para comprobar avisos.</summary>
        private sealed class EventLog : IEventObserver
        {
            public readonly List<GameEventType> Seen = new List<GameEventType>();
            public void UpdateOnEvent(GameEvent gameEvent) => Seen.Add(gameEvent.GetEventType());
        }

        [SetUp]
        public void Montar()
        {
            new ServiceRig();                  // pone el logger mudo...
            EventBus.GetInstance().Clear();    // ...y se descartan las suscripciones de sus sistemas

            // Catalogo vacio: desde que los objetos de desarrollo del jugador se meten desde
            // GameMain, EntityManager se construye sin el catalogo real.
            _entities = new EntityManager(new PrototypeFactory(new ItemCatalogue()));
            _world = new WorldInteractionSystem(_entities, new NullLinker());

            SystemManager systems = new SystemManager(_entities)
                .RegisterReactiveGameSystem(new InventorySystem())
                .RegisterReactiveGameSystem(_world);
            _service = new WorldInteractionService(new GameSystemContext(systems, new PresenterManager()));

            _events = new EventLog();
            EventBus.GetInstance().Subscribe(GameEventType.WeightLimitReached, _events);
            EventBus.GetInstance().Subscribe(GameEventType.InventoryFull, _events);
        }

        /// <summary>
        /// Un objetivo cuya superficie queda a <paramref name="distance"/> metros de los ojos de
        /// un actor sin cuerpo en el origen (ojos a 1,6 m). Esfera de radio 0,25 a la altura
        /// de los ojos, delante en +Z.
        /// </summary>
        private IEntity TargetAt(float distance)
        {
            InGameEntity target = new InGameEntity(IdGenerator.GenerateNewId());
            target.AddComponent(new PositionComponent(0f, 0f, distance + 0.25f));
            target.AddComponent(new InteractionVolumeComponent(new SphereVolume(0f, 1.6f, 0f, 0.25f)));
            _entities.Register(target);
            return target;
        }

        private static InGameEntity ActorAtOrigin()
        {
            InGameEntity actor = new InGameEntity(IdGenerator.GenerateNewId());
            actor.AddComponent(new PositionComponent(0f, 0f, 0f));
            return actor;
        }

        [Test]
        public void Seguir_en_rango_tiene_margen_sobre_el_alcance()
        {
            InGameEntity actor = ActorAtOrigin();

            Assert.IsTrue(_world.IsInRange(actor, TargetAt(1.9f)), "dentro del alcance (2,0)");
            Assert.IsTrue(_world.IsInRange(actor, TargetAt(2.3f)), "fuera del alcance pero dentro del margen (2,5)");
            Assert.IsFalse(_world.IsInRange(actor, TargetAt(2.7f)), "fuera del margen");
        }

        [Test]
        public void Algo_que_ya_no_esta_en_el_mundo_no_esta_en_rango()
        {
            InGameEntity actor = ActorAtOrigin();
            IEntity target = TargetAt(1f);
            _entities.RemoveEntity(target.GetIdAsInt());

            Assert.IsFalse(_world.IsInRange(actor, target));
        }

        [Test]
        public void Al_recoger_si_faltan_hueco_y_peso_el_motivo_es_el_peso()
        {
            // El inventario del actor es un contenedor de 1 celda y 1 kg. Contenedor y no cuerpo a
            // proposito: un cuerpo no frena por peso (CarryCapacity.GetTransferLimit), asi que
            // este caso solo existe con un actor sin cuerpo.
            InGameEntity actor = ActorAtOrigin();
            actor.AddComponent(new InventoryComponent(TestItems.InventoryOf(TestItems.Container(1, 1, maxWeight: 1f))));

            IEntity pile = _entities.CreateEntity(EntityType.GroundLot);
            pile.GetComponent<GroundLotComponent>().AddRange(new List<SubLot>
            {
                new SubLot(TestItems.Item(TestItems.SWORD, weight: 0.1f, maxStack: 1), 3),   // entra 1: frena la rejilla
                new SubLot(TestItems.Apple(weight: 1f), 2),                                   // no entra ninguna: frena el peso
            });

            _service.PickUp(actor, pile);

            Assert.Contains(GameEventType.WeightLimitReached, _events.Seen);
            Assert.IsFalse(_events.Seen.Contains(GameEventType.InventoryFull), "un solo motivo por gesto");
        }

        [Test]
        public void Tirar_una_unidad_la_deja_suelta_en_el_mundo()
        {
            InGameEntity dropper = ActorAtOrigin();

            _world.UpdateOnEvent(new ItemLotEvent(GameEventType.ItemDropped, dropper,
                new List<SubLot> { new SubLot(TestItems.Apple(), 1) }));

            Assert.IsEmpty(_entities.GetEntitiesWithComponent(typeof(GroundLotComponent)));
            List<IEntity> inWorld = _entities.GetEntitiesWithComponent(typeof(InteractionVolumeComponent));
            Assert.AreEqual(1, inWorld.Count);
            Assert.IsInstanceOf<ItemEntity>(inWorld[0]);
        }

        [Test]
        public void Tirar_varias_unidades_crea_un_monton()
        {
            InGameEntity dropper = ActorAtOrigin();

            _world.UpdateOnEvent(new ItemLotEvent(GameEventType.ItemDropped, dropper,
                new List<SubLot> { new SubLot(TestItems.Apple(), 3) }));

            List<IEntity> piles = _entities.GetEntitiesWithComponent(typeof(GroundLotComponent));
            Assert.AreEqual(1, piles.Count);
            Assert.AreEqual(3, piles[0].GetComponent<GroundLotComponent>().TotalUnits);
        }

        // ---- Cuanto cabe al recoger (M7 T7) ----

        private static InGameEntity ActorWithGrid(int h, int w)
        {
            InGameEntity actor = ActorAtOrigin();
            actor.AddComponent(new InventoryComponent(TestItems.InventoryOf(TestItems.Container(h, w, maxWeight: 100f))));
            return actor;
        }

        private static InventoryObject InventoryOfActor(IEntity actor)
            => actor.GetComponent<InventoryComponent>().Inventory;

        private IEntity PileOf(params SubLot[] lots)
        {
            IEntity pile = _entities.CreateEntity(EntityType.GroundLot);
            pile.GetComponent<GroundLotComponent>().AddRange(new List<SubLot>(lots));
            return pile;
        }

        /// Dos espadas de 1x2 llenan una rejilla 2x2; detras, tres manzanas que ya no caben.
        private IEntity SwordsThenApples()
            => PileOf(new SubLot(TestItems.Item(TestItems.SWORD, weight: 1f, maxStack: 1, w: 1, h: 2), 2),
                      new SubLot(TestItems.Apple(), 3));

        [Test]
        public void Lo_que_cuenta_CountPickable_es_lo_que_mueve_PickUp()
        {
            InGameEntity actor = ActorWithGrid(2, 2);
            IEntity pile = SwordsThenApples();

            int counted = _service.CountPickable(actor, pile);
            int left = _service.PickUp(actor, pile);

            Assert.AreEqual(2, counted, "las espadas llenan la rejilla y las manzanas ya no caben");
            Assert.AreEqual(counted, 5 - left, "contar y recoger tienen que coincidir");
        }

        [Test]
        public void Contar_no_toca_el_inventario_ni_el_monton()
        {
            InGameEntity actor = ActorWithGrid(2, 2);
            InventoryObject inventory = InventoryOfActor(actor);
            IEntity pile = SwordsThenApples();

            int freeBefore = inventory.GetGrid().GetFreeCellCount();
            float weightBefore = inventory.GetTotalWeight();

            _service.CountPickable(actor, pile);

            Assert.AreEqual(freeBefore, inventory.GetGrid().GetFreeCellCount());
            Assert.AreEqual(weightBefore, inventory.GetTotalWeight(), 1e-4f);
            Assert.AreEqual(5, WorldInteractionService.UnitsOn(pile));
        }

        [Test]
        public void Un_item_suelto_cuenta_uno_si_cabe_y_cero_si_no()
        {
            InGameEntity roomy = ActorWithGrid(2, 2);
            Assert.AreEqual(1, _service.CountPickable(roomy, TestItems.Apple()));

            InGameEntity full = ActorWithGrid(1, 1);
            InventoryOfActor(full).AddItem(TestItems.Bandage(), 1);   // ocupa la unica celda y no apila con manzanas
            Assert.AreEqual(0, _service.CountPickable(full, TestItems.Apple()));
        }

        [Test]
        public void Si_no_cabe_nada_cuenta_cero_y_recoger_lo_deja_todo()
        {
            InGameEntity actor = ActorWithGrid(1, 1);
            InventoryOfActor(actor).AddItem(TestItems.Bandage(), 1);
            IEntity pile = PileOf(new SubLot(TestItems.Apple(), 5));

            Assert.AreEqual(0, _service.CountPickable(actor, pile));
            Assert.AreEqual(5, _service.PickUp(actor, pile));
        }
    }
}
