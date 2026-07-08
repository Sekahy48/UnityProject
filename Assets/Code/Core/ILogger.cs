namespace Core
{
    /// <summary>
    /// Logging abstraction so Core classes don't depend on UnityEngine.Debug.
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
