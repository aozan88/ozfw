using KonFwLogging;
using Microsoft.Extensions.Logging;

namespace KonFwELK
{
    public class ELKLogger : IKonLogger
    {
        private readonly ELKConfiguration _config;
        private readonly Stack<LoggingScope> _scopes = new Stack<LoggingScope>();

        public ELKLogger(ELKConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Log(LogLevel level, string message, Exception exception = null, params object[] args)
        {
            if (level < _config.MinimumLogLevel) return;

            string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            string scopeInfo = GetCurrentScopeInfo();
            string indexName = string.Format(_config.IndexFormat, DateTime.UtcNow);

            Console.WriteLine($"[ELK {level}] {_config.ElasticUrl}/{indexName} - " +
                             $"{_config.ApplicationName}/{_config.Environment} - {scopeInfo}{formattedMessage}" +
                             $"{(exception != null ? $" - Exception: {exception.Message}" : "")}");
        }

        public void LogTrace(string message, params object[] args) =>
            Log(LogLevel.Trace, message, null, args);

        public void LogDebug(string message, params object[] args) =>
            Log(LogLevel.Debug, message, null, args);

        public void LogInformation(string message, params object[] args) =>
            Log(LogLevel.Information, message, null, args);

        public void LogWarning(string message, Exception exception = null, params object[] args) =>
            Log(LogLevel.Warning, message, exception, args);

        public void LogError(string message, Exception exception = null, params object[] args) =>
            Log(LogLevel.Error, message, exception, args);

        public void LogCritical(string message, Exception exception = null, params object[] args) =>
            Log(LogLevel.Critical, message, exception, args);

        public IDisposable BeginScope(string messageFormat, params object[] args)
        {
            if (!_config.IncludeScopes) return NullScope.Instance;

            var scope = new LoggingScope(this, string.Format(messageFormat, args));
            _scopes.Push(scope);
            return scope;
        }

        private string GetCurrentScopeInfo()
        {
            if (!_config.IncludeScopes || _scopes.Count == 0) return string.Empty;

            return string.Join(" => ", _scopes.Reverse().Select(s => s.Message)) + " => ";
        }

        private class LoggingScope : IDisposable
        {
            private readonly ELKLogger _logger;
            public string Message { get; }

            public LoggingScope(ELKLogger logger, string message)
            {
                _logger = logger;
                Message = message;
            }

            public void Dispose()
            {
                if (_logger._scopes.Count > 0)
                {
                    _logger._scopes.Pop();
                }
            }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            private NullScope() { }

            public void Dispose() { }
        }

    }
}