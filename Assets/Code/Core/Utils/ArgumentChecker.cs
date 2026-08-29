using System;

namespace Utils
{
    public static class ArgumentChecker
    {
        public static void CheckNotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName, "Argument Checker: " + argName + " no puede ser null");
            }
        }

        public static void CheckPositive(int number, string argName)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException("Argument Checker: " + argName + " debe ser positivo");
            }
        }
    }
}