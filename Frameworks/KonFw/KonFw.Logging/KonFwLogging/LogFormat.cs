namespace KonFwLogging
{
    public class LogFormat
    {
        public string LogLevel { get; set; } = "Information";
        public bool IncludeScopes { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/app.log";
    }
}
