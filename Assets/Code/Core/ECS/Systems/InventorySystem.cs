using System;
using System.Collections.Generic;
using Core;
using ECS.Component;
using ECS.Entity;
using Events;
using Inventory;
using Observer;
using AC = Utils.ArgumentChecker;

namespace ECS.Systems
{
    public class InventorySystem : IReactiveSystem
    {
        private static readonly GameEventType[] _subscribedEvents =
        {
            GameEventType.InventoryChanged 
        };

        public IEnumerable<GameEventType> SubscribedEvents => _subscribedEvents;


        /// <summary>
        /// Returns how many items can be added by weight, and outputs the InventoryComponent.
        /// Returns 0 if entity has no inventory.
        /// </summary>
        private int GetFitByWeight(IEntity entity, ItemEntity item, int amount, out InventoryComponent invComp)
        {
            invComp = entity.GetComponent<InventoryComponent>();
            if (invComp == null) return 0;

            float currentWeight = invComp.Inventory.GetTotalWeight();
            float itemWeight = item.GetComponent<BaseItemComponent>().GetWeight();
            float maxWeight = GetMaxWeight(entity);

            int fitByWeight = itemWeight > 0 ? (int)((maxWeight - currentWeight) / itemWeight) : amount;
            return Math.Min(amount, fitByWeight);
        }

        /// <summary>
        /// Tries to stack items checking weight first, then grid.
        /// Stacks onto existing compatible node if possible, creates new nodes for overflow.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryStackOntoHere(IEntity entity, ItemEntity item, int amount)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.StackOntoHere(item, toAdd);
            EvaluateAndFireEvents(entity, remaining > 0);   
            return remaining + (amount - toAdd);
        }

        /// <summary>
        /// Tries to stack items onto a specific node by nodeId, checking weight first.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryStackOntoNode(IEntity entity, ItemEntity item, int amount, int nodeId)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.StackOntoNode(nodeId, item, toAdd);
            EvaluateAndFireEvents(entity, false);
            return remaining + (amount - toAdd);
        }

        /// <summary>
        /// Tries to add items at a specific grid position, checking weight first.
        /// Returns the amount that could not be added.
        /// </summary>
        public int TryAddItemAt(IEntity entity, ItemEntity item, int amount, int row, int col)
        {
            int toAdd = GetFitByWeight(entity, item, amount, out InventoryComponent invComp);
            if (toAdd <= 0) return amount;
            int remaining = invComp.Inventory.AddItemAt(item, toAdd, row, col);
            EvaluateAndFireEvents(entity, remaining > 0);
            return remaining + (amount - toAdd);
        } 

        /// <summary>
        /// Moves units that already exist somewhere into a grid position, as a transaction.
        /// This is NOT the same job as the TryAdd* methods: those bring items in from outside
        /// (loot, crafting output, the dev catalog) where there is no source to subtract from.
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
                                 ItemEntity subLot, int amount, IEntity dstEntity, int row, int col)
        {
            AC.CheckNotNull(srcInventory, nameof(srcInventory));
            AC.CheckNotNull(srcNode, nameof(srcNode));
            AC.CheckPositive(amount, nameof(amount));

            // Lo extraido llega desglosado por variante: un nodo mezclado consume al azar,
            // y pasarle al destino un solo item convertiria las demas variantes en copias.
            List<(ItemEntity item, int amount)> taken =
                srcInventory.Extract(srcNode, subLot, amount, clean: false);

            int moved = 0;
            foreach ((ItemEntity variant, int count) in taken)
            {
                int leftover = TryAddItemAt(dstEntity, variant, count, row, col);
                moved += count - leftover;

                if (leftover > 0)
                    srcInventory.ModifyAmount(srcNode, variant, leftover, clean: false);
            }

            if (srcNode.GetAmount() <= 0)
                srcInventory.CleanNode(srcNode);

            if (srcEntity != null && srcEntity != dstEntity)
                EvaluateAndFireEvents(srcEntity, false);

            return moved;
        }

        public void EvaluateAndFireEvents(IEntity entity, bool fullGrid)
        {

            AC.CheckNotNull(entity, nameof(entity));
            InventoryComponent inventoryComponent = entity.GetComponent<InventoryComponent>();
            MovementComponent movementComponent = entity.GetComponent<MovementComponent>();
            AC.CheckNotNull(inventoryComponent, "inventoryComponent"); 
  
            float totalWeight = inventoryComponent.Inventory.GetTotalWeight();
            CoreLogger.Instance.Log("Total weight: " + totalWeight);

            // Physical carry capacity check
            if (entity.HasComponent(typeof(BodyComponent)) && movementComponent != null)
            {
                float carryWeight = GetMaxWeight(entity); 

                float weightRatio = carryWeight > 0 ? totalWeight / carryWeight : 1;

                GameEventType load = CarryCapacity.ClassifyLoad(weightRatio);
                EventBus.GetInstance().Post(new GameEvent(load, entity, movementComponent));
                LogLoad(load);

            }
            else if (entity.HasComponent(typeof(StorageComponent)))
            {
                // TODO: StorageComponent capacity checks
            }  

            if (fullGrid)
            {
                EventBus.GetInstance().Post(new GameEvent(GameEventType.InventoryFull , entity, inventoryComponent));
                CoreLogger.Instance.LogWarning("Inventory overflow: cannot transfere more object due to insufficient grid space.");
            }
        }

        /// <summary>
        /// Reports the encumbrance band. Separate from the classification itself:
        /// logging is a consequence of the band, not part of deciding it.
        /// </summary>
        private void LogLoad(GameEventType load)
        {
            switch (load)
            {
                case GameEventType.ExtraWeight:
                    CoreLogger.Instance.Log("Heavy load. Speed reduced.");
                    break;
                case GameEventType.Overweight:
                    CoreLogger.Instance.LogWarning("Overloaded. Speed heavily reduced, energy penalty.");
                    break;
                case GameEventType.Immobile:
                    CoreLogger.Instance.LogWarning("Cannot move due to excess weight.");
                    break;
                default:
                    CoreLogger.Instance.Log("No movement restrictions.");
                    break;
            }
        }

        private float GetMaxWeight(IEntity entity)
        {
            if (entity.HasComponent(typeof(BodyComponent)))
                return CarryCapacity.GetMaxCarryWeight(entity);

            StorageComponent storage = entity.GetComponent<StorageComponent>();
            return storage != null ? storage.MaxWeight : float.MaxValue;
        }

        public void ProcessEntity(IEntity entity)
        {
            EvaluateAndFireEvents(entity, false);
        } 
    
        public void UpdateOnEvent(GameEvent gameEvent)
        {
            if (gameEvent.GetEventType() == GameEventType.InventoryChanged)
            {
                ProcessEntity(gameEvent.GetEntity());
            }
        }
    }
}