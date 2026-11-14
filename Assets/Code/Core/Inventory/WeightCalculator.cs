using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// Cuanto pesa algo: un item (con lo que lleva dentro si es un contenedor), un monton o
    /// un cuerpo con lo que carga. Una sola respuesta para todo lo que pesa, la use quien la
    /// use: el inventario para decidir si cabe, la fisica para la masa (Fase 1.5, M2).
    ///
    /// <para>Existe porque la pregunta "cabe?" y la suma "cuanto llevas?" median cosas
    /// distintas. <see cref="InventoryObject.GetTotalWeight"/> ya contaba bien una mochila
    /// guardada —su peso mas su contenido—, pero quien decidia si podia entrar leia solo
    /// <c>BaseItemComponent.Weight</c>, el de la mochila vacia. Una mochila con diez kilos
    /// dentro entraba en tres kilos libres y, una vez dentro, pesaba once. Ahora las dos
    /// preguntas salen de aqui.</para>
    ///
    /// <para><b>Por que no en <c>CarryCapacityCalculator</c>.</b> Aquella contesta cuanto puede llevar
    /// un portador (musculo, hambre, fatiga, techo de un arcon); esta contesta cuanto pesa lo
    /// que se lleva. Son los dos lados de la misma comparacion, pero de duenos distintos:
    /// <c>CarryCapacityCalculator</c> lee componentes del cuerpo y esta destinada a volverse un sistema
    /// con su propio componente; el peso de un item es un hecho del arbol de inventario.
    /// Juntarlas tambien cerraria un ciclo: <c>InventoryObject</c> ya consulta
    /// <c>CarryCapacityCalculator.GetMaxLoad</c>, y esto necesita <c>InventoryObject.GetTotalWeight</c>.</para>
    ///
    /// <para><b>Por que en <c>Core/Inventory</c> y no junto a <c>ItemMagnitudeReader</c>.</b> Lo que
    /// anade sobre leer el campo es saber que un contenedor arrastra su contenido, y eso es
    /// conocimiento del Composite. En <c>Core/Item</c> haria que ese espacio dependiera del
    /// inventario, que a su vez ya depende de el.</para>
    /// </summary>
    public static class WeightCalculator
    {
        /// <summary>
        /// Peso de una unidad: el propio del item y, si es un contenedor, todo lo que cuelga
        /// de el. Un contenedor es siempre una unidad (<c>PlaceContainer</c> lo garantiza),
        /// asi que multiplicar este valor por unidades nunca cuenta dos veces un contenido.
        /// </summary>
        public static float Of(ItemEntity item)
        {
            if (item == null) return 0f;

            float own = item.GetComponent<BaseItemComponent>()?.Weight ?? 0f;

            InventoryObject contents = InventoryObject.ContainerOf(item);
            return contents == null ? own : own + contents.GetTotalWeight();
        }

        /// <summary>Peso de varias unidades del mismo item.</summary>
        public static float Of(ItemEntity item, int units) => Of(item) * units;

        /// <summary>
        /// Peso de cualquier entidad del juego:
        /// <list type="bullet">
        /// <item>un item: <see cref="Of(ItemEntity)"/>, con su contenido;</item>
        /// <item>un monton: lo que pesan todos sus lotes;</item>
        /// <item>un cuerpo: su peso mas el total de su inventario, que ya incluye lo que
        /// cuelga de la mochila puesta. La ropa puesta aun no pesa (decidido, sin aplicar);</item>
        /// <item>cualquier otra cosa: 0, que significa "no se sabe", no "no pesa". Quien
        /// necesite un minimo (la fisica no admite masa 0) lo pone el.</item>
        /// </list>
        ///
        /// <para>Comprueba primero si es un item porque con una variable de tipo
        /// <see cref="IEntity"/> se llega aqui aunque detras haya un item: la sobrecarga se
        /// elige por el tipo de la variable, no por el del objeto.</para>
        /// </summary>
        public static float Of(IEntity entity)
        {
            if (entity == null) return 0f;
            if (entity is ItemEntity item) return Of(item);

            GroundLotComponent pile = entity.GetComponent<GroundLotComponent>();
            if (pile != null) return pile.TotalWeight;

            float total = entity.GetComponent<BodyComponent>()?.Weight ?? 0f;
            InventoryComponent inventory = entity.GetComponent<InventoryComponent>();
            if (inventory != null) total += inventory.Inventory.GetTotalWeight();
            return total;
        }
    }
}
