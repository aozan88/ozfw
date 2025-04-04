using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace USBS.ServiceDefaults
{

    public static class DBManagementExtension
    {
        public static IServiceCollection AddDatabaseDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Register the configuration service
            services.AddScoped<IDefaultDbContextConfiguration>(provider =>
                new DefaultDbContextConfiguration(defaultConnectionString));

            return services;
        }
    }

    public class DefaultDbContextConfiguration : IDefaultDbContextConfiguration
    {
        public string ConnectionString { get; }

        public DefaultDbContextConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
