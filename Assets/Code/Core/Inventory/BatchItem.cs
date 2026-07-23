using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;

namespace Inventory
{
    /// <summary>
    /// Groups items of the same typeId into sub-lots differentiated by state (durability, condition, etc.).
    /// Each sub-lot is a pair (ItemEntity, amount). Items within the same sub-lot are Equivalent.
    /// Total amount across all sub-lots cannot exceed maxStackSize.
    /// </summary>
    public class BatchItem
    {
        private Random _random;
        private readonly List<(ItemEntity item, int amount)> _items;
        private readonly int _typeId;
        private readonly int _maxStackSize;

        public BatchItem(ItemEntity item, int amount)
        {
            _typeId = item.GetComponent<BaseItemComponent>().GetTypeId();
            _maxStackSize = item.GetComponent<BaseItemComponent>().GetMaxStackSize();

            _items = new List<(ItemEntity, int)>
            {
                (item, Math.Min(amount, _maxStackSize))
            };
 
            _random = new Random();
        }

        //#region Getters

        /// <summary>
        /// TypeId shared by all items in this batch.
        /// </summary>
        public int GetTypeId() => _typeId;

        /// <summary>
        /// Maximum total amount this batch can hold across all sub-lots.
        /// </summary>
        public int GetMaxStackSize() => _maxStackSize;

        /// <summary>
        /// Sum of amounts across all sub-lots.
        /// </summary>
        public int GetTotalAmount()
        {
            int total = 0;
            foreach ((ItemEntity item, int amount) pair in _items)
            {
                total += pair.amount;
            }
            return total;
        }

        /// <summary>
        /// Sum of (weight * amount) across all sub-lots.
        /// </summary>
        public float GetTotalWeight()
        {
            float totalWeight = 0;
            foreach ((ItemEntity item, int amount) pair in _items)
            {
                totalWeight += pair.item.GetComponent<BaseItemComponent>().GetWeight() * pair.amount;
            }
            return totalWeight;
        }

        /// <summary>
        /// Whether the batch has no sub-lots (all items consumed).
        /// </summary>
        public bool IsEmpty() => _items.Count == 0;

        /// <summary>
        /// Returns a copy of the sub-lot list for inspection.
        /// </summary>
        public List<(ItemEntity, int)> GetSubLots() => new List<(ItemEntity, int)>(_items);

        //#endregion

        //#region Add

        /// <summary>
        /// Adds a certain amount of an item if it matches the batch typeId.
        /// If the item is Equivalent to an existing sub-lot, merges into it.
        /// Otherwise creates a new sub-lot.
        /// </summary>
        /// <param name="item">Item to add (must share this batch's typeId).</param>
        /// <param name="amount">Number of units to add.</param>
        /// <returns>Amount that could not be added due to maxStackSize limit.</returns>
        public int AddAmount(ItemEntity item, int amount)
        {
            if (item.GetComponent<BaseItemComponent>().GetTypeId() != _typeId) return amount;

            int toAdd = Math.Min(amount, _maxStackSize - GetTotalAmount());
            int remaining = amount - toAdd;

            if (toAdd > 0 && !SetAmount(item, toAdd + GetBatchAmount(item)))
            {
                _items.Add((item, toAdd));
            }

            return remaining;
        }

        //#endregion

        //#region Consume

        /// <summary>
        /// Consumes 1 unit from a random sub-lot.
        /// </summary>
        /// <returns>The consumed ItemEntity, or null if the batch is empty.</returns>
        public ItemEntity ConsumeRandom()
        {
            if (_items.Count == 0) return null;

            int rndmIndex = _random.Next(0, _items.Count);
            ItemEntity item = _items[rndmIndex].item;
            bool consumed = ConsumeAmount(item, 1) != 0;
            return consumed ? item : null;
        }

        /// <summary>
        /// Consumes N units randomly across sub-lots. Returns what was consumed grouped by sub-lot.
        /// </summary>
        /// <param name="amount">Number of units to consume.</param>
        /// <returns>List of (ItemEntity, int) pairs representing what was consumed from each sub-lot.</returns>
        public List<(ItemEntity, int)> ConsumeRandom(int amount)
        {
            List<(ItemEntity item, int count)> consumed = new List<(ItemEntity, int)>();
            int toConsume = Math.Min(amount, GetTotalAmount());

            for (int i = 0; i < toConsume; i++)
            {
                if (_items.Count == 0) break;
                int rndmIndex = _random.Next(0, _items.Count);
                ItemEntity item = _items[rndmIndex].item;
                ConsumeAmount(item, 1);

                bool found = false;
                for (int j = 0; j < consumed.Count; j++)
                {
                    if (consumed[j].item.Equivalent(item))
                    {
                        consumed[j] = (consumed[j].item, consumed[j].count + 1);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    consumed.Add((item, 1));
            }

            return consumed;
        }

        /// <summary>
        /// Consumes up to the requested amount from the sub-lot matching the given item.
        /// </summary>
        /// <param name="item">Item identifying the sub-lot (matched by Equivalent).</param>
        /// <param name="amount">Maximum units to consume.</param>
        /// <returns>Actual amount consumed.</returns>
        public int ConsumeAmount(ItemEntity item, int amount)
        {
            int consumed = Math.Min(amount, GetBatchAmount(item));
            bool applied = SetAmount(item, GetBatchAmount(item) - consumed);
            return applied ? consumed : 0;
        }


        public void ConsumeAll()
        {
            _items.Clear();
        }
        //#endregion

        //#region Private helpers

        /// <summary>
        /// Returns the amount stored in the sub-lot whose item is Equivalent to the given one.
        /// </summary>
        private int GetBatchAmount(ItemEntity item)
        {
            foreach ((ItemEntity item, int amount) pair in _items)
            {
                if (pair.item.Equivalent(item)) return pair.amount;
            }
            return 0;
        }

        /// <summary>
        /// Sets the amount of the sub-lot matching the given item.
        /// If amount reaches 0, the sub-lot is removed.
        /// </summary>
        /// <returns>True if a matching sub-lot was found.</returns>
        private bool SetAmount(ItemEntity item, int amount)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var lot = _items[i];
                if (item.Equivalent(lot.item))
                {
                    if (amount <= 0)
                    {
                        _items.RemoveAt(i);
                    }
                    else
                    {
                        _items[i] = (lot.item, amount);
                    }
                    return true;
                }
            }
            return false;
        }

        //#endregion
    }
}
