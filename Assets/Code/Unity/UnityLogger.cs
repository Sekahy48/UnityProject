using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Implementación de ILogger que delega en UnityEngine.Debug.
    /// </summary>
    public class UnityLogger : Core.ILogger
    {
        public void Log(string message) => Debug.Log(message);
        public void LogWarning(string message) => Debug.LogWarning(message);
        public void LogError(string message) => Debug.LogError(message);
    }
}
