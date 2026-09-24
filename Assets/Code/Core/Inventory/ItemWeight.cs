using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// Cuanto pesa un item, contando lo que lleva dentro si es un contenedor.
    ///
    /// <para>Existe porque la pregunta "¿cabe?" y la suma "¿cuanto llevas?" median cosas
    /// distintas. <see cref="InventoryObject.GetTotalWeight"/> ya contaba bien una mochila
    /// guardada —su peso mas su contenido—, pero quien decidia si podia entrar leia solo
    /// <c>BaseItemComponent.Weight</c>, el de la mochila vacia. Una mochila con diez kilos
    /// dentro entraba en tres kilos libres y, una vez dentro, pesaba once. Ahora las dos
    /// preguntas salen de aqui.</para>
    ///
    /// <para><b>Por que no en <c>CarryCapacity</c>.</b> Aquella contesta cuanto puede llevar
    /// un portador (musculo, hambre, fatiga, techo de un arcon); esta contesta cuanto pesa lo
    /// que se lleva. Son los dos lados de la misma comparacion, pero de dueños distintos:
    /// <c>CarryCapacity</c> lee componentes del cuerpo y esta destinada a volverse un sistema
    /// con su propio componente; el peso de un item es un hecho del arbol de inventario.
    /// Juntarlas tambien cerraria un ciclo: <c>InventoryObject</c> ya consulta
    /// <c>CarryCapacity.GetMaxLoad</c>, y esto necesita <c>InventoryObject.GetTotalWeight</c>.</para>
    ///
    /// <para><b>Por que en <c>Core/Inventory</c> y no junto a <c>ItemMagnitudes</c>.</b> Lo que
    /// añade sobre leer el campo es saber que un contenedor arrastra su contenido, y eso es
    /// conocimiento del Composite. En <c>Core/Item</c> haria que ese espacio dependiera del
    /// inventario, que a su vez ya depende de el.</para>
    /// </summary>
    public static class ItemWeight
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
    }
}
