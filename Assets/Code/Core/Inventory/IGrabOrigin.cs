using System.Collections.Generic;
using Core.ECS.Entity;

namespace Core.Inventory
{
    /// <summary>
    /// De donde salen unas unidades agarradas. Un nodo de una rejilla y una capa de un slot
    /// de equipo responden a las mismas preguntas con implementaciones distintas, asi que
    /// la mano y las transferencias hablan con esto y no con un inventario concreto.
    /// </summary>
    public interface IGrabOrigin
    {
        /// <summary>Entidad dueña del origen. Su peso cambia al sacar unidades de aqui.</summary>
        IEntity Owner { get; }

        /// <summary>Item que representa lo que hay, para pintar la mano. Null si esta vacio.</summary>
        ItemEntity Representative { get; }

        /// <summary>Nodo cuyas celdas no deben estorbar al colocar. -1 cuando no aplica.</summary>
        int SourceNodeId { get; }

        /// <summary>Unidades disponibles de una variante, o del total si es null.</summary>
        int Available(ItemEntity variant = null);

        /// <summary>Saca unidades y devuelve que variantes salieron. No limpia el origen.</summary>
        IReadOnlyList<SubLot> Extract(ItemEntity variant, int amount);

        /// <summary>Devuelve al origen unidades que no llegaron a colocarse.</summary>
        void Restore(ItemEntity variant, int amount);

        /// <summary>Cierre de la transaccion: descarta el origen si se ha quedado vacio.</summary>
        void Clean();
    }
}