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
        /// For when saving games and needing to jump to a specific id number.
        /// </summary>
        public static void SetCurrentId(int id)
        {
            Interlocked.Exchange(ref currentId, id);
        }
    }
}