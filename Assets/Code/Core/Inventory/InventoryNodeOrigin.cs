using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    /// <summary>Un nodo concreto dentro de un InventoryObject.</summary>
    public class InventoryNodeOrigin : IGrabOrigin
    {
        private readonly IEntity _owner;
        private readonly InventoryObject _inventory;
        private readonly IInventoryElement _node;

        public InventoryNodeOrigin(IEntity owner, InventoryObject inventory, IInventoryElement node)
        {
            AC.CheckNotNull(inventory, nameof(inventory));
            AC.CheckNotNull(node, nameof(node));

            _owner = owner;
            _inventory = inventory;
            _node = node;
        }

        public IEntity Owner => _owner;
        public ItemEntity Representative => _node.GetItemEntity();
        public int SourceNodeId => _node.GetNodeId();

        /// <summary>Expuesto para comparar origen y destino: no es lo mismo reordenar que transferir.</summary>
        public InventoryObject Inventory => _inventory;
        public IInventoryElement Node => _node;

        /* Celda que ocupaba el nodo justo antes de salir. Solo significa algo para una rama:
           una hoja no se retira al extraerse parcialmente, asi que no hay sitio que recordar.
           Se captura ANTES de extraer porque despues la rejilla ya la ha liberado. */
        private GridPos _extractedFrom = GridPos.None;

        public int Available(ItemEntity variant = null) => _node.GetAmount(variant);

        // clean: false — los sobrantes de una hoja tienen que poder volver, y un nodo
        // "limpiado" habria que recrearlo en sus mismas coordenadas. Una rama se retira igual:
        // no se divide, asi que o salio entera o no salio.
        public IReadOnlyList<SubLot> Extract(ItemEntity variant, int amount)
        {
            if (!_node.IsLeaf())
                _extractedFrom = _inventory.GetGrid().GetElementOf(_node.GetNodeId())?.GetPos() ?? GridPos.None;

            return _inventory.ExtractFrom(_node, variant, amount, clean: false);
        }

        /// <summary>
        /// Devuelve al origen lo que el destino no acepto.
        ///
        /// Una hoja recupera unidades; una rama no tiene unidades que recuperar, asi que se
        /// vuelve a colgar donde estaba. Son dos operaciones distintas porque la salida tambien
        /// lo fue: de una hoja sale parte, de una rama sale ella.
        /// </summary>
        public void Restore(ItemEntity variant, int amount)
        {
            if (amount <= 0) return;

            if (!_node.IsLeaf())
            {
                _inventory.ReattachContainer((InventoryObject)_node, _extractedFrom);
                return;
            }

            // El cast es seguro por la rama de arriba, y la firma tipada a ItemObject es la que
            // impide que una rama llegue aqui por cualquier otro camino: las cuentas son cosa
            // de las hojas.
            _inventory.ModifyAmount((ItemObject)_node, variant, amount, clean: false);
        }

        /// <summary>
        /// Descarta el nodo si ya no queda nada de el.
        ///
        /// Solo aplica a las hojas: una rama ya se retiro en Extract, y una que siga aqui es
        /// una que no llego a salir. Un contenedor vacio, ademas, sigue siendo un contenedor.
        /// </summary>
        public void Clean()
        {
            if (_node.IsLeaf() && _node.GetAmount() <= 0) _inventory.CleanNode(_node);
        }
    }
}