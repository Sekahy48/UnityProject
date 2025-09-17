namespace ECS.Entity
{
    public static class IdGenerator
    {
        private static int currentId = 0;

        /// <summary>
        /// Generates a new unique ID.
        /// </summary>
        /// <returns>A unique integer ID.</returns>
        public static int GenerateNewId()
        {
            return currentId++;
        }

        /// <summary>
        /// Resets the ID generator to start from zero.
        /// </summary>
        public static void Reset()
        {
            currentId = 0;
        }
    }
}