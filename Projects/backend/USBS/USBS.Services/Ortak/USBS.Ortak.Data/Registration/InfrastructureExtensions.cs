using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using global::Ortak.Application.Contract;
using global::Ortak.Data.Repositories;
using Ortak.Data.Persistence;

namespace Ortak.Data.Registration
{

    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices<TContext>(this IServiceCollection services, string connectionString)
            where TContext : DbContext
        {
            // Register the DbContext
            services.AddDbContext<OrtakDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Register repositories (specific to the API)
            services.AddScoped<ISistemTanimRepository, SistemTanimRepository>();
            services.AddScoped<ISistemTanimTipiRepository, SistemTanimTipiRepository>();

            return services;
        }
    }
}
