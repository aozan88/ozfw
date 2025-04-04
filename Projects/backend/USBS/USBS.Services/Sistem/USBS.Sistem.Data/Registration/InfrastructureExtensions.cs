using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using global::Sistem.Application.Contract;
using global::Sistem.Data.Repositories;
using Sistem.Data.Persistence;

namespace Sistem.Data.Registration
{

    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices<TContext>(this IServiceCollection services, string connectionString)
            where TContext : DbContext
        {
            // Register the DbContext
            services.AddDbContext<SistemDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Register repositories (specific to the API)
            services.AddScoped<ISistemConfigRepository, SistemConfigRepository>();

            return services;
        }
    }
}
