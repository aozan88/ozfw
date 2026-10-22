using OzFwLogging;

namespace OzFwELK
{
    public class ELKConfiguration : LoggingConfiguration
    {
        public string ElasticUrl { get; set; } = "http://localhost:9200";
        public string KibanaUrl { get; set; } = "http://localhost:5601";
        public string IndexFormat { get; set; } = "ozfw-logs-{0:yyyy.MM.dd}";
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; } = false;
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
    }

}
