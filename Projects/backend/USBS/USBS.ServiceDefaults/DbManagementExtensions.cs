using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Common.DbContexts;
using USBS.Components.DbManagement;

namespace USBS.ServiceDefaults.DBManagement
{
    public static class DbManagementExtensions
    {
        public static IServiceCollection AddDatabaseDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbManagement<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            return services;
        }
    }
}
