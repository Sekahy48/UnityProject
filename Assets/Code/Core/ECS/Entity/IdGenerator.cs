using System.Threading;

namespace ECS.Entity
{
    public static class IdGenerator
    {
        private static int currentId = 0;

        /// <summary>
        /// Generates a new unique ID. Thread-safe.
        /// </summary>
        /// <returns>A unique integer ID.</returns>
        public static int GenerateNewId()
        {
            return Interlocked.Increment(ref currentId) - 1;
        }

        /// <summary>
        /// Para cuando haya que guardar partidas y se necesite ir a un punto concreto de numero de id.
        /// </summary>
        public static void SetCurrentId(int id)
        {
            Interlocked.Exchange(ref currentId, id);
        }
    }
}