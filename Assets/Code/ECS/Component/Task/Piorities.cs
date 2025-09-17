namespace ECS.Component.Task
{
    public enum Priorities
    {
        OMNIP = 0,
        HIGH = 1,
        MEDIUM = 2,
        LOW = 3,
        MINIMAL = 4
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
