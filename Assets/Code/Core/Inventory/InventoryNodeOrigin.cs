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
        private readonly ItemObject _node;

        public InventoryNodeOrigin(IEntity owner, InventoryObject inventory, ItemObject node)
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
        public ItemObject Node => _node;

        public int Available(ItemEntity variant = null) => _node.GetAmount(variant);

        // clean: false — los sobrantes tienen que poder volver, y un nodo "limpiado" habria
        // que recrearlo en sus mismas coordenadas.
        public IReadOnlyList<SubLot> Extract(ItemEntity variant, int amount)
            => _inventory.Extract(_node, variant, amount, clean: false);

        public void Restore(ItemEntity variant, int amount)
            => _inventory.ModifyAmount(_node, variant, amount, clean: false);

        public void Clean()
        {
            if (_node.GetAmount() <= 0) _inventory.CleanNode(_node);
        }
    }
}