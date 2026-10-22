using OzFwLogging;

namespace OzFwGraylog
{
    public class GraylogConfiguration : LoggingConfiguration
    {
        public string Host { get; set; } = "https://graylog.konumsal.com.tr";
        public int Port { get; set; } = 12201;
        public string Facility { get; set; } = "OzFw";
        public bool UseUdp { get; set; } = true;
        public int MaxUdpMessageSize { get; set; } = 8192;
        public Dictionary<string, string> AdditionalFields { get; set; } = new Dictionary<string, string>();
    }

}
