namespace Core.Events
{
    public enum GameEventType
    {
        InventoryChanged,
        ExtraWeight,
        Overweight,
        Immobile,
        InventoryFull,
        NormalWeight,
        EquipmentChanged,
        ItemDropped,

        /// <summary>
        /// Algo no entro en un inventario porque se acababa el peso. Pareja de
        /// <see cref="InventoryFull"/>, que es lo mismo por falta de rejilla. No confundir con
        /// <see cref="ExtraWeight"/>/<see cref="Overweight"/>/<see cref="Immobile"/>, que
        /// dicen cuanto vas cargado, no que algo se haya quedado fuera.
        /// </summary>
        WeightLimitReached,
    }
}