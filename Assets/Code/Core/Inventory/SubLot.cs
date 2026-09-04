using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// Un lote parcial dentro de un <see cref="BatchItem"/>: una variante concreta del item y
    /// cuantas unidades hay de ella. Antes viajaba como tupla anonima
    /// <c>(ItemEntity, int)</c>, que ya era un tipo valor — lo que se gana aqui no es
    /// rendimiento, es nombre: <c>List&lt;SubLot&gt;</c> dice lo que es, y da un sitio donde
    /// colgar operaciones como <see cref="TotalWeight"/>, hasta ahora recalculada a mano en
    /// cada punto que la necesitaba.
    ///
    /// <para><b>Invariante heredada de BatchItem:</b> la entidad es inmutable mientras este
    /// aqui. Una misma <see cref="ItemEntity"/> puede compartirse entre varios sub-lotes, asi
    /// que mutarla alteraria en silencio todas las pilas que la comparten. Para cambiar el
    /// estado de unas unidades: clonar, restar del lote original y anadir el clon como lote
    /// nuevo.</para>
    /// </summary>
    public readonly struct SubLot 
    {
        public readonly ItemEntity Item;
        public readonly int Amount;

        public SubLot(ItemEntity item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        /// <summary>Peso de las unidades de este lote.</summary>
        public float TotalWeight =>
            Item == null ? 0f : Item.GetComponent<BaseItemComponent>().Weight * Amount;

        /// <summary>
        /// Permite seguir escribiendo <c>foreach ((ItemEntity variante, int unidades) in lotes)</c>,
        /// que es como se lee mejor en los bucles que ya existian.
        /// </summary>
        public void Deconstruct(out ItemEntity item, out int amount)
        {
            item = Item;
            amount = Amount;
        }

        public bool Equivalent(SubLot other)
        {
            return Item.Equivalent(other.Item);
        }
    }
}
