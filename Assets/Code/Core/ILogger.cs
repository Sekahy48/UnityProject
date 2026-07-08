namespace Core
{
    /// <summary>
    /// Abstracción de logging para que las clases Core no dependan de UnityEngine.Debug.
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
