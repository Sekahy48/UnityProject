using System;
using System.Collections.Generic;
using Core;
using Core.Contexts;
using ECS.Component;
using ECS.Entity;
using ECS.Systems;
using Inventory;
using AC = Utils.ArgumentChecker;

namespace Services
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
        public int GrabFrom(IEntity entity, InventoryObject inventory, ItemObject node, int amount, ItemEntity subLot = null)
        {
            return _interactionContext._handBuffer.Grab(entity, inventory, node, amount, subLot);
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
            return _interactionContext._handBuffer.Grab(null, staging, node, amount, null);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destiny"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns> What is left in the hand</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int PlaceAmountFromHand(IEntity destiny, int row, int col)
        {
            HandBuffer hand = _interactionContext._handBuffer;
            ItemObject srcNode = hand.GetSourceNode();

            int ignoreNodeId = GetIgnoreNodeId(destiny);

            int moved = TryMoveItemTo(hand.GetEntity(), hand.GetSourceInventory(), srcNode,
                                      hand.GetHeldSubLot(), hand.GetHeldAmount(), destiny, row, col,
                                      ignoreNodeId);
            CoreLogger.Instance.Log(moved.ToString());
            int handMoved = hand.NotifyPlaced(moved);
            if (moved != handMoved) 
                throw new InvalidOperationException("Amount moved in the real inventory doesn't math the amount moved in the hand.");
            else    
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
        /// mid-transaction, holding its cells — harmless because nothing observes it, and
        /// because CanPlace lets a node overlap its own cells.</para>
        /// </summary>
        /// <param name="srcEntity">Entity owning the source inventory. Its weight drops, so it
        /// must re-evaluate too: unloading into a chest would otherwise leave the carrier's
        /// overweight debuff applied, since TryAddItemAt only fires events for the destination.</param>
        /// <param name="srcInventory">Container holding the node. Must be its direct parent.</param>
        /// <param name="srcNode">Node the units come from.</param>
        /// <param name="subLot">Variant to move (matched by Equivalent), or null to take at random
        /// across the node — whatever comes out is what travels, variants preserved.</param>
        /// <param name="amount">Units to move.</param>
        /// <param name="dstEntity">Entity owning the destination inventory. May be the same as the source's.</param>
        /// <param name="row">Destination row.</param>
        /// <param name="col">Destination column.</param>
        /// <returns>Units actually moved. Zero means nothing changed anywhere.</returns>
        public int TryMoveItemTo(IEntity srcEntity, InventoryObject srcInventory, ItemObject srcNode,
                                 ItemEntity subLot, int amount, IEntity dstEntity, int row, int col,
                                 int ignoreNodeId = -1)
        {
            AC.CheckNotNull(srcInventory, nameof(srcInventory));
            AC.CheckNotNull(srcNode, nameof(srcNode));
            AC.CheckPositive(amount, nameof(amount));

            // Antes de extraer nada: una celda fuera de rango haria saltar AddItemAt a mitad
            // de la transaccion, con las unidades ya fuera del nodo origen y el rollback sin
            // ejecutar. Soltar fuera de la grid no es un error, simplemente no coloca.
            InventoryObject dstInventory = dstEntity.GetComponent<InventoryComponent>().Inventory;
            if (!dstInventory.GetGrid().IsInside(row, col)) return 0;

            InventorySystem inventorySystem = _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();

            // Lo extraido llega desglosado por variante: un nodo mezclado consume al azar,
            // y pasarle al destino un solo item convertiria las demas variantes en copias.
            List<(ItemEntity item, int amount)> taken =
                srcInventory.Extract(srcNode, subLot, amount, clean: false);

            int moved = 0;
            foreach ((ItemEntity variant, int count) in taken)
            {
                int leftover = inventorySystem.TryAddItemAt(dstEntity, variant, count, row, col, ignoreNodeId);
                moved += count - leftover;

                if (leftover > 0)
                    srcInventory.ModifyAmount(srcNode, variant, leftover, clean: false);
            }

            if (srcNode.GetAmount() <= 0)
                srcInventory.CleanNode(srcNode);

            if (srcEntity != null && srcEntity != dstEntity)
                inventorySystem.EvaluateAndFireEvents(srcEntity, false);

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
            ItemObject srcNode = hand.GetSourceNode();
            if (srcNode == null) return -1;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            bool sameInventory = ReferenceEquals(hand.GetSourceInventory(), dstInventory);
            bool emptiesSource = hand.GetHeldAmount() >= srcNode.GetAmount();
            return sameInventory && emptiesSource ? srcNode.GetNodeId() : -1;
        }

        /// <summary>
        /// Que pasaria si la mano se soltase en (row, col). Recorre las MISMAS decisiones que
        /// AddItemAt/TryAddItemAt y en el mismo orden: ocupante primero, luego hueco, luego
        /// peso. Si esto y la colocacion real dejan de coincidir es que una de las dos cambio
        /// sola, y el color estaria mintiendo.
        /// </summary>
        public PlacementVerdict EvaluatePlacement(IEntity destiny, int row, int col)
        {
            if (destiny == null || !IsHandCarrying()) return PlacementVerdict.Outside;

            InventoryObject dstInventory = destiny.GetComponent<InventoryComponent>().Inventory;
            TetrisGridState grid = dstInventory.GetGrid();
            if (!grid.IsInside(row, col)) return PlacementVerdict.Outside;

            ItemEntity item = GetGrabbedItem();
            if (item == null) return PlacementVerdict.Outside;

            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            int ignoreNodeId = GetIgnoreNodeId(destiny);
            int amount = _interactionContext._handBuffer.GetHeldAmount();

            // Mismo orden que AddItemAt: el ocupante manda sobre el hueco.
            GridElement occupant = grid.GetElementAt(row, col);
            if (occupant != null && occupant.GetNode().GetNodeId() != ignoreNodeId)
            {
                ItemObject node = occupant.GetNode();
                bool sameType = node.GetTypeId() == baseInfo.TypeId;
                bool hasRoom  = node.GetAmount() < baseInfo.MaxStackSize;
                return sameType && hasRoom ? PlacementVerdict.Fits : PlacementVerdict.Blocked;
            }

            if (!grid.CanPlace(row, col, baseInfo.DimensionH, baseInfo.DimensionW, ignoreNodeId))
                return PlacementVerdict.Blocked;

            // Reordenar dentro de un inventario no cambia su peso: ya se carga. Igual que
            // TryAddItemAt, que se salta la comprobacion cuando hay nodo ignorado.
            if (ignoreNodeId == -1 &&
                CarryCapacity.FitByWeight(destiny, dstInventory, item, amount) <= 0)
                return PlacementVerdict.Blocked;

            return PlacementVerdict.Fits;
        }

        public bool IsHandCarrying() => !_interactionContext._handBuffer.IsEmpty();

        /// <summary>Node the held units still sit in, or null when the hand is empty.</summary>
        public ItemObject GetGrabbedNode() => _interactionContext._handBuffer.GetSourceNode();
        public void EmptyHand() => _interactionContext._handBuffer.Clear();
        public ItemEntity GetGrabbedItem()
        {   
            HandBuffer hand = _interactionContext._handBuffer;
            ItemEntity item = hand.GetHeldSubLot();
            if (item != null) return item;

            // Mano vacia: no hay nodo del que sacar el representante.
            ItemObject node = GetGrabbedNode();
            return node?.GetItemEntity();
        }

        public int GetGrabbedAmount() => _interactionContext._handBuffer.GetHeldAmount();
    }
}