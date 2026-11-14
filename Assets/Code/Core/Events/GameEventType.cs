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
        EnergyChanged,

        /// <summary>
        /// El peso de la entidad del aviso PUEDE haber cambiado: se publica en cada operacion
        /// capaz de cambiarlo (inventario, equipo, recoger parte de un monton), sin comprobar
        /// si de verdad cambio. Quien lo escuche tiene que aguantar avisos repetidos; el peso
        /// nuevo se pregunta a <see cref="Core.Inventory.WeightCalculator.Of(Core.ECS.Entity.IEntity)"/>.
        /// No sustituye a <see cref="InventoryChanged"/>: mover algo dentro de la rejilla
        /// cambia el inventario y no el peso, y recoger de un monton cambia el peso sin que
        /// haya inventario.
        /// </summary>
        WeightChanged,
    }
}