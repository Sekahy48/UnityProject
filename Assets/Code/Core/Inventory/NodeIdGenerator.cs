namespace Inventory
{
    /// <summary>
    /// Generates unique IDs for composite tree nodes (ItemObject, InventoryObject).
    /// Independent from entity IDs. Non-persistent — resets each session.
    /// </summary>
    public static class NodeIdGenerator
    {
        private static int _next = 1;

        public static int GenerateId() => _next++;

        public static void Reset() => _next = 1;
    }
}
