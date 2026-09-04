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
        /// <returns> What is left in the hand</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int PlaceAmountFromHand(IEntity destiny, GridPos pos)
        {
            int ignoreNodeId = GetIgnoreNodeId(destiny);

            int leftover = PlaceFromHand(destiny, (variant, count) =>
                _systemContext.SystemManager.GetReactiveSystem<InventorySystem>()
                    .TryAddItemAt(destiny, variant, count, pos, ignoreNodeId, announce: false));

            return leftover;
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
                _systemContext.SystemManager.GetReactiveSystem<EquipmentSystem>()
                    .TryEquip(destiny, variant, slot) == EquipResult.SuccessEquip ? 0 : count);
        }

        /// <summary>
        /// Descarga la mano sobre un destino cualquiera. Lo unico que cambia entre destinos es
        /// como se colocan las unidades, asi que eso llega como parametro.
        /// </summary>
        /// <returns>Lo que queda en la mano.</returns>
        private int PlaceFromHand(IEntity destiny, Func<ItemEntity, int, int> place)
        {
            HandBuffer hand = _interactionContext._handBuffer;
            IGrabOrigin origin = hand.GetOrigin();
            if (origin == null) return 0;

            int moved = RunTransfer(origin, hand.GetHeldSubLot(), hand.GetHeldAmount(), destiny, place);

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
        public int TryEquipItem(IGrabOrigin origin, ItemEntity subLot,
                                IEntity dstEquipmentEntity, List<EquipmentSlotType> dstEquipmentSlots)
        {
            Func<ItemEntity, int, int> addFunction = (variant, count) =>
            {
                EquipmentSystem equipmentSystem = _systemContext.SystemManager.GetReactiveSystem<EquipmentSystem>();

                // El equipo no admite parciales: o entra la prenda o no cabe nada.
                return equipmentSystem.TryEquip(dstEquipmentEntity, variant, dstEquipmentSlots) == EquipResult.SuccessEquip ? 0 : count;
            };

            return RunTransfer(origin, subLot, 1, dstEquipmentEntity, addFunction);
        }

        /// <summary>
        /// Quita una prenda del equipo y la mete en el inventario de su dueño, como
        /// transaccion: si no cabe, vuelve al equipo.
        /// </summary>
        /// <param name="pos">Celda concreta, o null para apilar donde quepa.</param>
        /// <returns>1 si la prenda acabo en el inventario, 0 si volvio al equipo.</returns>
        public int TryUnequipItem(IEntity srcUnequipEntity, ItemEntity equipmentItem,
                                  List<EquipmentSlotType> srcEquipmenSlots, GridPos? pos = null)
        {
            IGrabOrigin origin = EquipmentOrigin(srcUnequipEntity, srcEquipmenSlots, equipmentItem);

            Func<ItemEntity, int, int> addFunction = (variant, count) =>
            {
                InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();

                return pos == null
                    ? inventorySystem.TryStackOntoHere(srcUnequipEntity, variant, count, false)
                    : inventorySystem.TryAddItemAt(srcUnequipEntity, variant, count, pos.Value, -1, false);
            };

            int moved = RunTransfer(origin, null, 1, srcUnequipEntity, addFunction);

            return moved;
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
        /// Que pasaria si la mano se soltase en esa celda. Recorre las MISMAS decisiones que
        /// AddItemAt/TryAddItemAt y en el mismo orden: ocupante primero, luego hueco, luego
        /// peso. Si esto y la colocacion real dejan de coincidir es que una de las dos cambio
        /// sola, y el color estaria mintiendo.
        /// </summary>
        public PlacementVerdict EvaluatePlacement(IEntity destiny, GridPos pos)
        {
            if (destiny == null || !IsHandCarrying()) return PlacementVerdict.Outside;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            TetrisGridState grid = dstInventory.GetGrid();
            if (!grid.IsInside(pos)) return PlacementVerdict.Outside;

            ItemEntity item = GetGrabbedItem();
            if (item == null) return PlacementVerdict.Outside;

            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            int ignoreNodeId = GetIgnoreNodeId(destiny);
            int amount = _interactionContext._handBuffer.GetHeldAmount();

            // Mismo orden que AddItemAt: el ocupante manda sobre el hueco.
            GridElement occupant = grid.GetElementAt(pos);
            if (occupant != null && occupant.GetNode().GetNodeId() != ignoreNodeId)
            {
                ItemObject node = occupant.GetNode();
                bool sameType = node.GetTypeId() == baseInfo.TypeId;
                bool hasRoom  = node.GetAmount() < baseInfo.MaxStackSize;
                return sameType && hasRoom ? PlacementVerdict.Fits : PlacementVerdict.Blocked;
            }

            if (!grid.CanPlace(pos, baseInfo.DimensionH, baseInfo.DimensionW, ignoreNodeId))
                return PlacementVerdict.Blocked;

            // Reordenar dentro de un inventario no cambia su peso: ya se carga. Igual que
            // TryAddItemAt, que se salta la comprobacion cuando hay nodo ignorado.
            if (ignoreNodeId == -1 &&
                CarryCapacity.FitByWeight(destiny, dstInventory, item, amount) <= 0)
                return PlacementVerdict.Blocked;

            return PlacementVerdict.Fits;
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

            return equipment.CanEquip(slots, item, wearable.FullOcupancy);
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

        public void DropItems(IEntity origin, ItemObject node, int amount, ItemEntity item = null)
        {
            AC.CheckNotNull(origin, nameof(origin));
            AC.CheckNotNull(node, nameof(node));
            AC.CheckPositive(amount, nameof(amount));
            
            InventoryObject inventory = origin.GetComponent<InventoryComponent>().Inventory;
            if (inventory.FindNodeById(node.GetNodeId()) == null)
                throw new InvalidOperationException("Cannot drop items from a pair entity-node if the provided node is not contained in the inventory of the entity.");

            IReadOnlyList<SubLot> items;
            items = inventory.Extract(node, item, amount);

            _systemContext.SystemManager.GetReactiveSystem<InventorySystem>().EvaluateAndFireEvents(origin, false);
            EventBus.GetInstance().Post(new ItemLotEvent(GameEventType.ItemDropped, origin, items)); 
        } 

        public List<ItemAction> GetAvailableActions(ItemEntity target, IEntity owner, IEntity source)
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
        
    }
}