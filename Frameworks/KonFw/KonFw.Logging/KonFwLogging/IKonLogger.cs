using Microsoft.Extensions.Logging;

namespace KonFwLogging
{
    public interface IKonLogger
    {
        void Log(LogLevel level, string message, Exception exception = null, params object[] args);
        void LogTrace(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, Exception exception = null, params object[] args);
        void LogError(string message, Exception exception = null, params object[] args);
        void LogCritical(string message, Exception exception = null, params object[] args);

        IDisposable BeginScope(string messageFormat, params object[] args);

    }

}
