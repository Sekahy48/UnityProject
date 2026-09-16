using System;
using Core.ECS.Component.Equipment;
using Core.ECS.Entity;
using Core.Inventory;
using Core.MVC.View.UI.Inventory;
using MVC.View.Inventory;

namespace Core.MVC.Presenter.Inventory
{
    /// <summary>
    /// De donde salio un menu contextual: todo lo que sus opciones necesitan saber para actuar
    /// cuando se pulsen, mucho despues de que el clic que las abrio haya desaparecido.
    ///
    /// Existe porque la lista de parametros crecia con cada accion nueva —origen, nodo,
    /// variante, slot, ancla, panel, celda— y se propagaba entera por RenderContextualMenu y
    /// BuildOptions aunque cada accion use dos o tres. Agrupados tienen ademas un nombre para
    /// lo que son: el contexto del gesto.
    ///
    /// <para>Los dos origenes posibles son excluyentes y se hacen inconstruibles, igual que la
    /// hoja y la rama de <see cref="MenuOption"/>: una fabrica por origen, asi que no existe
    /// forma de escribir un contexto sin nodo ni variante, ni uno con las dos cosas.</para>
    /// </summary>
    public readonly struct MenuContext
    {
        /// <summary>Entidad dueña de lo que se va a manipular.</summary>
        public readonly IEntity Origin;

        /// <summary>Pila concreta sobre la que se abrio. Null cuando viene del equipamiento.</summary>
        public readonly IInventoryElement Target;

        /// <summary>Prenda equipada sobre la que se abrio. Null cuando viene de una rejilla.</summary>
        public readonly ItemEntity Item;

        /// <summary>Slot del que salio la prenda. Null cuando viene de una rejilla.</summary>
        public readonly EquipmentSlotType? SlotType;

        /// <summary>Punto al que se anclara lo que abra una opcion. Cero si no hay ancla.</summary>
        public readonly PanelPoint Anchor;

        /// <summary>Panel de la rejilla de origen. Null cuando viene del equipamiento.</summary>
        public readonly PanelType? Panel;

        /// <summary>Celda pulsada. None cuando viene del equipamiento.</summary>
        public readonly GridPos Cell;

        private MenuContext(IEntity origin, IInventoryElement target, ItemEntity item,
                            EquipmentSlotType? slotType, PanelPoint anchor,
                            PanelType? panel, GridPos cell)
        {
            Origin = origin;
            Target = target;
            Item = item;
            SlotType = slotType;
            Anchor = anchor;
            Panel = panel;
            Cell = cell;
        }

        /// <summary>Abierto sobre una celda de rejilla.</summary>
        public static MenuContext FromGrid(IEntity origin, IInventoryElement target, PanelType panel,
                                           GridPos cell, PanelPoint anchor)
        {
            if (target == null || target.GetItemEntity() == null)
                throw new InvalidOperationException("Cannot open a contextual menu over an empty cell.");

            return new MenuContext(origin, target, null, null, anchor, panel, cell);
        }

        /// <summary>Abierto sobre una prenda equipada.</summary>
        public static MenuContext FromEquipment(IEntity origin, ItemEntity item, EquipmentSlotType slotType)
        {
            if (item == null)
                throw new InvalidOperationException("Cannot open a contextual menu over an empty equipment slot.");

            return new MenuContext(origin, null, item, slotType, default, null, GridPos.None);
        }

        /// <summary>Abierto sobre una variante concreta dentro de una pila.</summary>
        public static MenuContext FromSublot(IEntity origin, ItemObject target, ItemEntity variant,
                                     PanelType panel, GridPos cell, PanelPoint anchor)
        {
            if (target == null || target.GetItemEntity() == null || variant == null)
                throw new InvalidOperationException("Cannot open a contextual menu over an empty sublot.");

            return new MenuContext(origin, target, variant, null, anchor, panel, cell);
        }

        /// <summary>
        /// Unidades de lo enfocado: el sub-lote si se abrio sobre una variante, la pila entera
        /// si se abrio sobre la celda, y una sola si es una prenda equipada.
        /// </summary>
        public int FocusedAmount =>
              Target == null ? 1
            : Item == null   ? Target.GetAmount()
            :                  Target.GetAmount(Item);

        /// <summary>
        /// Lo que el menu esta enfocando, venga de donde venga: una rejilla enfoca la pila
        /// entera, el equipamiento una prenda suelta.
        ///
        /// Ya en datos de presentacion. Una pila pasa por BuildNodeData para que
        /// lleve lo que solo el nodo sabe de si mismo —hoy si tiene varias variantes—; una
        /// prenda equipada no es un nodo y no tiene nada de eso que contar.
        ///
        /// <para>Se recalcula en cada lectura: quien lo use en un camino repetido —el hover del
        /// menu, por ejemplo— debe guardarselo, porque GetSubLots devuelve lista nueva.</para>
        /// </summary>
        public ItemDisplayData FocusedDisplayData =>
            Target == null ? DisplayDTOsBuilder.BuildDisplayData(Item, 1)
            : Item == null   ? DisplayDTOsBuilder.BuildNodeData(Target)
            :                  DisplayDTOsBuilder.BuildDisplayData(Item, Target.GetAmount(Item));
    
        /// <summary>
        /// La pila enfocada, o null si lo enfocado no es una pila — un contenedor o una prenda no
        /// tienen sub-lotes que desglosar.
        /// </summary>
        public ItemObject TargetStack => Target as ItemObject;
    }
}
