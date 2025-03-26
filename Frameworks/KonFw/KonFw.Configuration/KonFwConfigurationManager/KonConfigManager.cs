using Microsoft.Extensions.Configuration;

namespace KonFwConfigurationManager
{
    public interface IKonConfigManager
    {
        T GetConfig<T>(string sectionName) where T : class, new();
    }

    public class KonConfigManager : IKonConfigManager
    {
        private readonly IConfiguration _configuration;

        public KonConfigManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T GetConfig<T>(string sectionName) where T : class, new()
        {
            var config = new T();
            //_configuration.GetSection(sectionName).Bind(config);
            return config;
        }
    }
}
