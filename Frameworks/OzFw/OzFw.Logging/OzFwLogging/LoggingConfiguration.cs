using Microsoft.Extensions.Logging;

namespace OzFwLogging
{
    public class LoggingConfiguration
    {
        public string LogLevel { get; set; } = "Information";
        public bool IncludeScopes { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/app.log";
        public LogLevel MinimumLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
        public string ApplicationName { get; set; } = "OzFwApplication";
        public string Environment { get; set; } = "Development";
    }
}
