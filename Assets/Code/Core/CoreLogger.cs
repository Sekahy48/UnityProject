namespace Core
{
    /// <summary>
    /// Static accessor for the logger. Set Instance once at startup
    /// from Unity (CoreLogger.Instance = new UnityLogger()).
    /// Core classes use CoreLogger.Instance.Log(...) without needing
    /// ILogger injected via constructor.
    /// </summary>
    public static class CoreLogger
    {
        public static ILogger Instance { get; set; }
    }
}
