using OzFwELK;
using OzFwLogging;
using Microsoft.Extensions.Logging;

namespace USBS.Components.Logging
{
    public class ELKLoggerProvider : IKonLogger
    {
        private readonly ELKConfiguration _config;

        public ELKLoggerProvider(ELKConfiguration config)
        {
            _config = config;
        }

        public void Log(LogLevel level, string message, Exception exception = null, params object[] args)
        {
            //Console.WriteLine($"[Graylog {level}] {_config.Host}:{_config.Port} - {message}");
        }
        public void LogTrace(string message, params object[] args) => Log(LogLevel.Trace, message, args: args);
        public void LogDebug(string message, params object[] args) => Log(LogLevel.Debug, message, args: args);
        public void LogInformation(string message, params object[] args) => Log(LogLevel.Information, message, args: args);
        public void LogWarning(string message, Exception exception = null, params object[] args) => Log(LogLevel.Warning, message, exception, args);
        public void LogError(string message, Exception exception = null, params object[] args) => Log(LogLevel.Error, message, exception, args);
        public void LogCritical(string message, Exception exception = null, params object[] args) => Log(LogLevel.Critical, message, exception, args);

        public IDisposable BeginScope(string messageFormat, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
