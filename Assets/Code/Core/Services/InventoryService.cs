using System;
using System.Collections.Generic; 
using Core.Contexts;
using Core.ECS.Component;
using Core.ECS.Component.Equipment;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.Inventory;  
using AC = Core.Utils.ArgumentChecker;

namespace Core.Services
{
    public class InventoryService
    {
        private readonly GameInteractionContext _interactionContext;
        private readonly GameSystemContext _systemContext;

        public InventoryService(GameInteractionContext interactionContext,
                                GameSystemContext systemContext)
        {
            _interactionContext = interactionContext;
            _systemContext = systemContext;
        }

        /// <returns>Units actually grabbed, clamped to what the node holds.</returns>
        public int GrabFrom(IGrabOrigin grabOrigin, int amount, ItemEntity subLot = null)
        {
            return _interactionContext._handBuffer.Grab(grabOrigin, amount, subLot);
        }

        /// <summary>Suma unidades al agarre en curso. Ver HandBuffer.GrabMore.</summary>
        public int GrabMore(int amount) => _interactionContext._handBuffer.GrabMore(amount);

        /// <summary>Unidades del origen que la mano aun no tiene reservadas.</summary>
        public int GetUngrabbedAmount() => _interactionContext._handBuffer.Ungrabbed();

        /// <summary>
        /// Si lo que se lleva en la mano salio de ese nodo. Se resuelve contra la rejilla en el
        /// momento de preguntarlo y no contra nada recordado, asi que abandonar el item y
        /// volver sigue contando como el mismo origen.
        /// </summary>
        public bool IsGrabbedFrom(IInventoryElement node)
            => node != null && IsHandCarrying() && GetGrabbedNodeId() == node.GetNodeId();

        /// <summary>
        /// Puts an amount of a certain item into the hand buffer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns> The amount actually held by the hand </returns>
        public int SpawnIntoHand(ItemEntity item, int amount)
        {
            ItemObject node = new ItemObject(item, amount);
            InventoryObject staging = new InventoryObject();
            staging.AddNode(node);
            return _interactionContext._handBuffer.Grab(new InventoryNodeOrigin(null, staging, node), amount);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destiny"></param>
        /// <param name="pos">Destination cell.</param>
        /// <param name="amount">Unidades a colocar, o null para toda la mano.</param>
        /// <returns> What is left in the hand</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int PlaceAmountFromHand(IEntity destiny, GridPos pos, int? amount = null)
        {
            int ignoreNodeId = GetIgnoreNodeId(destiny);

            int leftover = PlaceFromHand(destiny, (variant, count) =>
                _systemContext.SystemManager.GetReactiveSystem<InventorySystem>()
                    .TryAddItemAt(destiny, variant, count, pos, ignoreNodeId, announce: false), amount);

            return leftover;
        }

        /// <summary>
        /// Si lo que se lleva en la mano puede intercambiarse con ese nodo: cada uno a las
        /// celdas del otro.
        ///
        /// Condiciones, y el motivo de cada una:
        /// - El origen es un nodo de rejilla. Desde el equipo o el catalogo no hay hueco de
        ///   salida al que mandar el nodo desplazado.
        /// - Mismo inventario. Entre paneles el intercambio mueve peso, y eso necesita las dos
        ///   mitades en una transaccion; esto no lo es.
        /// - La mano lleva el nodo ENTERO. Con media pila agarrada el origen sigue ocupando sus
        ///   celdas, asi que no hay hueco que ofrecer a cambio.
        /// - Y los dos caben en su destino sin pisarse entre ellos.
        /// </summary>
        /// <param name="pos">Celda pulsada. Ahi va la esquina del nodo de la mano: lo que se
        /// lleva se coloca donde apuntas, como en cualquier otra colocacion, y no en la esquina
        /// del nodo desplazado. Ese se conforma con la esquina que queda libre.</param>
        public bool CanSwapWith(IEntity destiny, GridPos pos)
        {
            if (!IsHandCarrying()) return false;
            if (!(_interactionContext._handBuffer.GetOrigin() is InventoryNodeOrigin origin)) return false;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            if (!ReferenceEquals(origin.Inventory, dstInventory)) return false;

            TetrisGridState grid = dstInventory.GetGrid();
            IInventoryElement target = grid.GetElementAt(pos)?.GetNode();
            if (target == null) return false;

            IInventoryElement held = origin.Node;
            if (ReferenceEquals(held, target)) return false;
            if (_interactionContext._handBuffer.Ungrabbed() != 0) return false;

            GridElement elemHeld = grid.GetElementOf(held.GetNodeId());
            if (elemHeld == null) return false;

            GridPos dstHeld = pos;
            GridPos dstTarget = elemHeld.GetPos();

            if (!Fits(grid, held, dstHeld, held.GetNodeId(), target.GetNodeId())) return false;
            if (!Fits(grid, target, dstTarget, held.GetNodeId(), target.GetNodeId())) return false;

            // Las dos huellas de DESTINO tampoco pueden pisarse entre ellas. Comprobar cada
            // colocacion por separado ignora al otro nodo, asi que una espada que empieza
            // pegada a la manzana con la que se cambia pasa las dos comprobaciones y luego
            // colisiona contra ella al colocar la segunda. Con esto el orden deja de importar.
            return !Overlap(dstHeld, Footprint(held), dstTarget, Footprint(target));
        }

        private static BaseItemComponent Footprint(IInventoryElement node)
            => node.GetItemEntity().GetComponent<BaseItemComponent>();

        private static bool Fits(TetrisGridState grid, IInventoryElement node, GridPos pos,
                                 int ignoreNodeId, int alsoIgnoreNodeId)
        {
            BaseItemComponent baseInfo = Footprint(node);

            return grid.CanPlace(pos, baseInfo.DimensionH, baseInfo.DimensionW,
                                 ignoreNodeId, alsoIgnoreNodeId);
        }

        /// <summary>Si dos rectangulos de celdas comparten alguna.</summary>
        private static bool Overlap(GridPos posA, BaseItemComponent a, GridPos posB, BaseItemComponent b)
            => posA.Row < posB.Row + b.DimensionH && posB.Row < posA.Row + a.DimensionH
            && posA.Col < posB.Col + b.DimensionW && posB.Col < posA.Col + a.DimensionW;

        /// <summary>
        /// Intercambia el nodo que se lleva en la mano con el que ocupa el destino. Arranca
        /// llamando a CanSwapWith, que es lo mismo que consulta la UI para pintar el azul.
        ///
        /// La mano se vacia con Clear y no con NotifyPlaced: no se ha colocado nada en el
        /// sentido de la mano, el nodo entero se ha movido por debajo. Y es seguro porque lo
        /// reservado nunca habia salido de ese nodo.
        /// </summary>
        public bool SwapFromHand(IEntity destiny, GridPos pos)
        {
            if (!CanSwapWith(destiny, pos)) return false;

            InventoryNodeOrigin origin = (InventoryNodeOrigin)_interactionContext._handBuffer.GetOrigin();
            TetrisGridState grid = destiny.GetComponent<InventoryComponent>().Inventory.GetGrid();
            IInventoryElement target = grid.GetElementAt(pos).GetNode();

            if (!grid.SwapNodes(origin.Node, pos, target)) return false;

            _interactionContext._handBuffer.Clear();
            _systemContext.SystemManager.GetReactiveSystem<InventorySystem>()
                .EvaluateAndFireEvents(destiny, false);

            return true;
        }

        /// <summary>
        /// Separa unidades de un nodo en una pila nueva dentro del mismo inventario, en el
        /// primer hueco que la admita.
        ///
        /// Va por RunTransfer y no por un apaño propio porque partir una pila es una
        /// transferencia como cualquier otra: origen, destino y vuelta atras si el destino no
        /// acepta todo. Que origen y destino sean la misma entidad no cambia nada.
        /// </summary>
        /// <param name="variant">Sub-lote a separar, o null para tomar del nodo al azar.</param>
        /// <returns>Unidades que no pudieron separarse.</returns>
        public int SplitNode(IEntity owner, IInventoryElement node, ItemEntity variant, int amount)
        {
            AC.CheckNotNull(owner, nameof(owner));
            AC.CheckNotNull(node, nameof(node));
            AC.CheckPositive(amount, nameof(amount));

            InventoryObject inventory = owner.GetComponent<InventoryComponent>().Inventory;
            GridPos free = FindSplitCell(inventory, variant ?? node.GetItemEntity());
            if (free.IsNone) return amount;

            return RunTransfer(new InventoryNodeOrigin(owner, inventory, node), variant, amount, owner,
                (v, count) => _systemContext.SystemManager.GetReactiveSystem<InventorySystem>()
                                  .TryAddItemAt(owner, v, count, free, -1, announce: false));
        }

        /// <summary>
        /// Hueco donde caeria una pila separada de este item, o None si la rejilla no tiene
        /// sitio. Expuesto porque la decision de ofrecer o no la accion de dividir depende de
        /// la misma respuesta: sin hueco la opcion no debe aparecer.
        /// </summary>
        public GridPos FindSplitCell(InventoryObject inventory, ItemEntity item)
        {
            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();

            return inventory.GetGrid().FindFirstFit(baseInfo.DimensionH, baseInfo.DimensionW);
        }

        /// <summary>
        /// Descarga la mano sobre un slot de equipo.
        ///
        /// Arranca preguntando a EvaluateEquip, que es lo mismo que consulta la UI para
        /// pintar el fantasma. Sin eso los dos caminos divergen: una prenda de ocupacion
        /// completa va a TODOS sus slots, asi que la lista no depende de donde sueltes, y
        /// soltar una pechera sobre los pies la equipaba en el pecho mientras el fantasma
        /// se pintaba en rojo.
        /// </summary>
        /// <param name="hovered">Slot sobre el que se ha soltado.</param>
        /// <returns>Lo que queda en la mano.</returns>
        public int EquipFromHand(IEntity destiny, EquipmentSlotType hovered, List<EquipmentSlotType> slot)
        {
            if (EvaluateEquip(destiny, hovered, slot) != EquipResult.SuccessEquip)
                return _interactionContext._handBuffer.GetHeldAmount();

            // El equipo no admite parciales: o entra la prenda o no cabe nada.
            return PlaceFromHand(destiny, (variant, count) =>
            {
                EquipResult result = _systemContext.SystemManager.GetReactiveSystem<EquipmentSystem>()
                                         .TryEquip(destiny, variant, slot);
                if (result != EquipResult.SuccessEquip) return count;

                WornContainers.Attach(destiny, variant);

                return 0;
            });
        }

        /// <summary>
        /// Descarga la mano sobre un destino cualquiera. Lo unico que cambia entre destinos es
        /// como se colocan las unidades, asi que eso llega como parametro.
        /// </summary>
        /// <param name="amount">Unidades a colocar, o null para toda la mano.</param>
        /// <returns>Lo que queda en la mano.</returns>
        private int PlaceFromHand(IEntity destiny, Func<ItemEntity, int, int> place, int? amount = null)
        {
            HandBuffer hand = _interactionContext._handBuffer;
            IGrabOrigin origin = hand.GetOrigin();
            if (origin == null) return 0;

            int toPlace = Math.Min(amount ?? hand.GetHeldAmount(), hand.GetHeldAmount());
            if (toPlace <= 0) return hand.GetHeldAmount();

            int moved = RunTransfer(origin, hand.GetHeldSubLot(), toPlace, destiny, place);

            int handMoved = hand.NotifyPlaced(moved);
            if (moved != handMoved)
                throw new InvalidOperationException(
                    "Amount moved in the real inventory doesn't match the amount moved in the hand.");

            return hand.GetHeldAmount();
        }

         /// <summary>
        /// Moves units that already exist somewhere into a grid position, as a transaction.
        /// This is NOT the same job as the TryAdd* methods: those bring items in from outside
        /// (loot, crafting output, the dev catalog) where there is no source to subtract from, or the source is not relevant (a "transfer
        /// all type interaction for example).
        /// Here both ends exist, so the operation needs an origin, a rollback, and care with
        /// double counting. It wraps TryAddItemAt rather than replacing it.
        ///
        /// <para>Order matters. Units are removed from the source FIRST, so that while the
        /// destination validates weight and stack limits they are no longer counted at the
        /// origin. Adding first would make a move inside one inventory fail against its own
        /// weight, and dropping a stack back where it came from hit maxStackSize against
        /// itself — both counted twice for the length of the operation.</para>
        ///
        /// <para>The source node is NOT cleaned until the end: leftovers have to go back, and
        /// a cleaned node would have to be recreated at its old coordinates. It may sit empty
        /// mid-transaction, holding its cells — harmless because, internally, the methods of
        /// the inventory system are  guarded by a boolean parameter that keeps the invocation of the method in this case free 
        /// of calling UpdateAndFireEvents, and because CanPlace lets a node overlap its own cells.</para>
        /// </summary>
        /// <param name="origin">De donde salen las unidades. Su dueño reevalua peso tambien:
        /// descargar en un arcon dejaria si no el debuff de sobrepeso puesto al portador,
        /// porque la colocacion solo dispara eventos para el destino.</param>
        /// <param name="subLot">Variant to move (matched by Equivalent), or null to take at random
        /// across the node — whatever comes out is what travels, variants preserved.</param>
        /// <param name="amount">Units to move.</param>
        /// <param name="dstEntity">Entity owning the destination inventory. May be the same as the source's.</param>
        /// <param name="pos">Destination cell.</param>
        /// <returns>Units actually moved. Zero means nothing changed anywhere.</returns>
        public int TryMoveItemTo(IGrabOrigin origin, ItemEntity subLot, int amount,
                                 IEntity dstEntity, GridPos pos, int ignoreNodeId = -1)
        {
            // Antes de extraer nada: una celda fuera de rango haria saltar AddItemAt a mitad
            // de la transaccion, con las unidades ya fuera del nodo origen y el rollback sin
            // ejecutar. Soltar fuera de la grid no es un error, simplemente no coloca.
            InventoryObject dstInventory = dstEntity.GetComponent<InventoryComponent>().Inventory;
            if (!dstInventory.GetGrid().IsInside(pos)) return 0;

            Func<ItemEntity, int, int> addFunction = (variant, count) => {
                InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();
                return inventorySystem.TryAddItemAt(dstEntity, variant, count, pos, ignoreNodeId, false);
            };

            return RunTransfer(origin, subLot, amount, dstEntity, addFunction);
        }

        public int TryQuickTransfer(IGrabOrigin origin, ItemEntity subLot, int amount, IEntity dstEntity)
        {
            Func<ItemEntity, int, int> addFunction = (variant, count) => {
                InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();
                return inventorySystem.TryStackOntoHere(dstEntity, variant, count, false);
            };

            return RunTransfer(origin, subLot, amount, dstEntity, addFunction);
        }

        //NOTA considerar cambiar el tipo de retorno a EquipmentResult
        public int TryEquipItem(IGrabOrigin origin, ItemEntity equipmentItem,
                                IEntity dstEquipmentEntity, List<EquipmentSlotType> dstEquipmentSlots)
        {
            AC.CheckNotNull(origin, nameof(origin));
            AC.CheckNotNull(equipmentItem, nameof(equipmentItem));
            AC.CheckNotNull(dstEquipmentEntity, nameof(dstEquipmentEntity));
            AC.CheckNotNull(dstEquipmentSlots, nameof(dstEquipmentSlots));

            Func<ItemEntity, int, int> addFunction = (variant, count) =>
            {
                EquipmentSystem equipmentSystem = _systemContext.SystemManager.GetReactiveSystem<EquipmentSystem>();

                // El equipo no admite parciales: o entra la prenda o no cabe nada.
                EquipResult result = equipmentSystem.TryEquip(dstEquipmentEntity, variant, dstEquipmentSlots, false);
                if (result != EquipResult.SuccessEquip) return count;

                WornContainers.Attach(dstEquipmentEntity, variant);

                return 0;
            };

            int equiped = RunTransfer(origin, equipmentItem, 1, dstEquipmentEntity, addFunction); 
            if (equiped > 0) 
                EventBus.GetInstance().Post(new GameEvent(GameEventType.EquipmentChanged, dstEquipmentEntity, dstEquipmentEntity.GetComponent<EquipmentComponent>()));
            return equiped;
        }

        /// <summary>
        /// Quita una prenda del equipo y la mete en el inventario de su dueño, como
        /// transaccion: si no cabe, vuelve al equipo.
        /// </summary>
        /// <param name="pos">Celda concreta, o null para apilar donde quepa.</param>
        /// <returns>1 si la prenda acabo en el inventario, 0 si volvio al equipo.</returns>
        public int TryUnequipItem(IEntity srcUnequipEntity, ItemEntity equipmentItem,
                                  List<EquipmentSlotType> srcEquipmentSlots, GridPos? pos = null)
        {
            IGrabOrigin origin = EquipmentOrigin(srcUnequipEntity, srcEquipmentSlots, equipmentItem);

            Func<ItemEntity, int, int> addFunction = (variant, count) =>
            {
                InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();

                return pos == null
                    ? inventorySystem.TryStackOntoHere(srcUnequipEntity, variant, count, false)
                    : inventorySystem.TryAddItemAt(srcUnequipEntity, variant, count, pos.Value, -1, false);
            };

            // El desenganche lo hace EquipmentSlotOrigin.Extract, que es por donde salen TODOS
            // los desequipados — este y el de agarrar una capa con la mano.
            return RunTransfer(origin, null, 1, srcUnequipEntity, addFunction);
        }

        private int RunTransfer(IGrabOrigin origin, 
                                 ItemEntity subLot, int amount, IEntity dstEntity, Func<ItemEntity, int, int> addAcction) 
        {
            AC.CheckNotNull(origin, nameof(origin));
            AC.CheckPositive(amount, nameof(amount));
            InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();

            // Lo extraido llega desglosado por variante: un nodo mezclado consume al azar,
            // y pasarle al destino un solo item convertiria las demas variantes en copias.
            IReadOnlyList<SubLot> taken = origin.Extract(subLot, amount);

            int moved = 0;
            foreach ((ItemEntity variant, int count) in taken)
            {
                int leftover = addAcction(variant, count);
                moved += count - leftover;

                if (leftover > 0)
                    origin.Restore(variant, leftover);
            }

            // El origen decide si se descarta: un nodo vacio libera sus celdas, un slot de
            // equipo no tiene nada que liberar.
            origin.Clean();

            inventorySystem.EvaluateAndFireEvents(dstEntity, moved < amount);
            if (origin.Owner != null && origin.Owner != dstEntity)
                inventorySystem.EvaluateAndFireEvents(origin.Owner, false);

            return moved;
        }

        /// <summary>
        /// Nodo cuyas celdas cuentan como libres para este movimiento. Un nodo que se empuja
        /// sobre celdas que ya ocupa chocaria consigo mismo, y solo es legitimo cuando va a
        /// desaparecer de esa rejilla: mover PARTE de una pila deja el origen vivo y sus celdas
        /// ocupadas de verdad.
        /// </summary>
        private int GetIgnoreNodeId(IEntity destiny)
        {
            HandBuffer hand = _interactionContext._handBuffer;

            // Solo un nodo de rejilla ocupa celdas que puedan estorbarle a su propia colocacion.
            // Mano vacia u origen de equipo: nada que ignorar.
            if (!(hand.GetOrigin() is InventoryNodeOrigin nodeOrigin)) return -1;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            bool sameInventory = ReferenceEquals(nodeOrigin.Inventory, dstInventory);
            bool emptiesSource = hand.GetHeldAmount() >= nodeOrigin.Available();

            return sameInventory && emptiesSource ? nodeOrigin.SourceNodeId : -1;
        }

        /// <summary>
        /// Inventario del que sale lo que se lleva en la mano, o null si no sale de ninguna rejilla.
        /// Hermano de GetIgnoreNodeId y de GetIgnoredEquipItem: lo que la mano tiene reservado sigue
        /// contando donde estaba, y quien evalua necesita saberlo para no contarlo dos veces.
        /// </summary>
        private InventoryObject GrabbedSourceInventory()
            => _interactionContext._handBuffer.GetOrigin() is InventoryNodeOrigin origin ? origin.Inventory : null;

        /// <summary>
        /// Si lo que impide meter esas unidades en ese destino es el techo de quien lo lleva, y
        /// no el suyo propio.
        ///
        /// Vive aqui y no en el presenter para que use el mismo origen que EvaluatePlacement:
        /// el aviso y el color tienen que descontar lo mismo, o saldra el cartel justo cuando
        /// el fantasma diga que si cabe.
        /// </summary>
        public bool CarrierBlocks(IEntity destiny, ItemEntity item, int units)
        {
            if (destiny == null || item == null) return false;

            float attempted = item.GetComponent<BaseItemComponent>().Weight * units;

            return destiny.GetComponent<InventoryComponent>().Inventory
                          .CarrierBlocks(attempted, GrabbedSourceInventory());
        }
            
        /// <summary>
        /// Que pasaria si la mano se soltase en esa celda. Recorre las MISMAS decisiones que
        /// AddItemAt/TryAddItemAt y en el mismo orden: ocupante primero, luego hueco, luego
        /// peso. Si esto y la colocacion real dejan de coincidir es que una de las dos cambio
        /// sola, y el color estaria mintiendo.
        /// </summary>
        /// <param name="amount">Unidades a evaluar, o null para toda la mano.</param>
        public PlacementVerdict EvaluatePlacement(IEntity destiny, GridPos pos, int? amount = null)
        {
            if (destiny == null || !IsHandCarrying()) return PlacementVerdict.Outside;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            TetrisGridState grid = dstInventory.GetGrid();
            if (!grid.IsInside(pos)) return PlacementVerdict.Outside;

            ItemEntity item = GetGrabbedItem();
            if (item == null) return PlacementVerdict.Outside;

            int held = _interactionContext._handBuffer.GetHeldAmount();
            int requested = Math.Min(amount ?? held, held);

            int landing = UnitsThatWouldLand(destiny, dstInventory, grid, pos, item, requested,
                                             GetIgnoreNodeId(destiny));

            if (landing > 0)
                return landing < requested ? PlacementVerdict.Partial : PlacementVerdict.Fits;

            // Ultimo recurso antes de dar por bloqueado: donde no cabe nada todavia puede caber
            // un intercambio, y eso es otra respuesta, no un no.
            return CanSwapWith(destiny, pos) ? PlacementVerdict.Swap : PlacementVerdict.Blocked;
        }

        /// <summary>
        /// Unidades que aterrizarian de verdad al soltar.
        ///
        /// El veredicto sale de este numero en vez de decidirse a trozos con un return por
        /// guarda: asi ninguna rama puede olvidarse de una regla. Pasaba exactamente eso —
        /// apilar sobre una pila compatible salia por su propia rama y nunca llegaba a
        /// comprobar el peso, asi que el fantasma pintaba verde y no se movia nada.
        /// </summary>
        private int UnitsThatWouldLand(IEntity destiny, InventoryObject inventory, TetrisGridState grid,
                                       GridPos pos, ItemEntity item, int requested, int ignoreNodeId)
        {
            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();

            // Un contenedor no entra en si mismo ni en nada que lleve dentro: seria un ciclo en
            // el arbol. Se pregunta aqui ademas de en AddItemAt porque el fantasma tiene que
            // poder decirlo antes de soltar.
            InventoryObject carried = item.GetComponent<InventoryComponent>()?.Inventory;
            if (carried != null && carried.WrapsOrIs(inventory)) return 0;
 
            int byWeight = inventory.FitByWeight(item, requested, GrabbedSourceInventory());

            // Mismo orden que AddItemAt: el ocupante manda sobre el hueco.
            GridElement occupant = grid.GetElementAt(pos);
            if (occupant != null && occupant.GetNode().GetNodeId() != ignoreNodeId)
            {
                IInventoryElement node = occupant.GetNode();
                if (node.GetTypeId() != baseInfo.TypeId) return 0;

                int room = baseInfo.MaxStackSize - node.GetAmount();

                return Math.Min(byWeight, Math.Max(room, 0));
            }

            if (!grid.CanPlace(pos, baseInfo.DimensionH, baseInfo.DimensionW, ignoreNodeId))
                return 0;

            return Math.Min(byWeight, baseInfo.MaxStackSize);
        }

        /// <summary>
        /// Que pasaria si la mano se soltase sobre esos slots de equipo. Hermana de
        /// EvaluatePlacement: pura, barata (se llama en cada PointerMove) y por el MISMO
        /// camino que el equipado real — EquipItem arranca llamando a CanEquip, asi que el
        /// veredicto y la operacion no pueden discrepar.
        ///
        /// Devuelve EquipResult y no un veredicto de UI porque el motivo del rechazo se
        /// conserva: hoy solo se pinta un color, pero ahi esta el "ya llevas una camisa".
        /// </summary>
        /// <param name="hovered">Slot sobre el que se esta preguntando. Con ocupacion completa
        /// la prenda va a TODOS sus slots, asi que la lista no depende de donde apuntes — pero
        /// la pregunta si: soltar una pechera sobre la cabeza no la equipa aunque el pecho
        /// este libre.</param>
        public EquipResult EvaluateEquip(IEntity destiny, EquipmentSlotType hovered, List<EquipmentSlotType> slots)
        {
            if (destiny == null || slots == null || slots.Count == 0) return EquipResult.NoSlotFits;
            if (!slots.Contains(hovered)) return EquipResult.WrongSlot;

            ItemEntity item = GetGrabbedItem();
            if (item == null) return EquipResult.NotWearable;

            EquipmentComponent equipment = destiny.GetComponent<EquipmentComponent>();
            if (equipment == null) return EquipResult.NoSlotFits;

            WearableComponent wearable = item.GetComponent<WearableComponent>();
            if (wearable == null) return EquipResult.NotWearable;

            return equipment.CanEquip(slots, item, wearable.FullOcupancy, GetIgnoredEquipItem());
        }

        /// <summary>
        /// Prenda que sigue puesta pero se considera de paso porque se lleva en la mano.
        /// Hermano de GetIgnoreNodeId: la mano es una referencia y no saca nada de su sitio
        /// hasta colocar, asi que devolver al slot lo que acabas de sacar de el chocaria
        /// contra si mismo. Se resuelve aqui y no en el llamante para que la consulta y la
        /// ejecucion no puedan usar criterios distintos.
        /// </summary>
        private ItemEntity GetIgnoredEquipItem()
        {
            return _interactionContext._handBuffer.GetOrigin() is EquipmentSlotOrigin origin
                ? origin.Representative
                : null;
        }

        public bool IsHandCarrying() => !_interactionContext._handBuffer.IsEmpty();

        /// <summary>
        /// Nodo en el que siguen las unidades agarradas. -1 con la mano vacia o cuando lo
        /// agarrado no vive en una rejilla (una prenda equipada), asi que nunca coincide
        /// con un nodo real y sirve directamente para decidir que bloque se pinta atenuado.
        /// </summary>
        public int GetGrabbedNodeId() => _interactionContext._handBuffer.GetOrigin()?.SourceNodeId ?? -1;
        public void EmptyHand() => _interactionContext._handBuffer.Clear();

        /// <summary>De donde salio lo que se lleva en la mano. Null con la mano vacia.</summary>
        public IGrabOrigin GetGrabbedOrigin() => _interactionContext._handBuffer.GetOrigin();
        public ItemEntity GetGrabbedItem()
        {   
            HandBuffer hand = _interactionContext._handBuffer;
            return hand.GetHeldItem();
        }

        public int GetGrabbedAmount() => _interactionContext._handBuffer.GetHeldAmount();

        public void DropItems(IEntity origin, IInventoryElement node, int amount, ItemEntity item = null)
        {
            AC.CheckNotNull(origin, nameof(origin));
            AC.CheckNotNull(node, nameof(node));
            AC.CheckPositive(amount, nameof(amount));
            
            InventoryObject inventory = origin.GetComponent<InventoryComponent>().Inventory;
            if (inventory.FindNodeById(node.GetNodeId()) == null)
                throw new InvalidOperationException("Cannot drop items from a pair entity-node if the provided node is not contained in the inventory of the entity.");

            IReadOnlyList<SubLot> items;
            items = inventory.ExtractFrom(node, item, amount);

            _systemContext.SystemManager.GetReactiveSystem<InventorySystem>().EvaluateAndFireEvents(origin, false);
            EventBus.GetInstance().Post(new ItemLotEvent(GameEventType.ItemDropped, origin, items)); 
        } 

        /// <param name="hasVariants">La pila tiene mas de una variante dentro: habilita
        /// inspeccionar su desglose.</param>
        /// <param name="splittable">La pila tiene mas de una unidad Y hay hueco donde dejar la
        /// mitad separada. Las dos condiciones juntas porque una accion que no puede cumplirse
        /// no debe ofrecerse: dividir sin hueco no tiene forma de explicar por que no pasa nada.</param>
        public List<ItemAction> GetAvailableActions(ItemEntity target, IEntity owner, IEntity source,
                                                   bool hasVariants, bool splittable = false)
        {

            List<ItemAction> options = new List<ItemAction>();
            if (target == null) return options;

            EquipmentComponent equipmentComponent = owner.GetComponent<EquipmentComponent>(); 
            if (equipmentComponent != null && target.GetComponent<WearableComponent>() != null)
            {
                if (equipmentComponent.HasEquiped(target))
                    options.Add(ItemAction.Unequip);
                else
                {
                    options.Add(ItemAction.Equip);
                    options.AddRange(new List<ItemAction>{ItemAction.DropFromInventory, ItemAction.QuickTransfer});
                }
            } else
            {
                options.AddRange(new List<ItemAction>{ItemAction.DropFromInventory, ItemAction.QuickTransfer});
            }

            if (hasVariants)
                options.Add(ItemAction.Inspect);

            if (splittable)
                options.Add(ItemAction.Split);
                
            
            
            // NOTA: el consume vendra con la salud y la nutricion
            return options;
        }

        public IGrabOrigin NodeOrigin(IEntity owner, InventoryObject inventoryObject, ItemObject itemObject)
        {
            return new InventoryNodeOrigin(owner, inventoryObject, itemObject);
        }

        public IGrabOrigin EquipmentOrigin(IEntity owner, List<EquipmentSlotType> slotTypes, ItemEntity item)
        {
            return new EquipmentSlotOrigin(owner, slotTypes, item, _systemContext.SystemManager.GetReactiveSystem<EquipmentSystem>());
        }
    }

    public enum ItemAction
    {
        DropFromInventory,
        QuickTransfer,
        Equip,
        Unequip,
        Consume,
        Inspect,
        Split,

    }
}