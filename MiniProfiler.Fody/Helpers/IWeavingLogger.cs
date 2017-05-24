namespace MiniProfiler.Fody.Helpers
{
    internal interface IWeavingLogger
    {
        void LogDebug(string message);

        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}