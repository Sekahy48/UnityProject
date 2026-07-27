namespace ECS.Component.Task
{
    public enum Priorities
    {
        Omnip = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        Minimal = 4
    }

    public static class PrioritiesExtensions
    {
        public static int GetValue(this Priorities priority)
        {
            return (int)priority;
        }

        public static bool EqualsPriority(this Priorities priority, Priorities other)
        {
            return priority == other || priority.GetValue() == other.GetValue();
        }
    }
}
