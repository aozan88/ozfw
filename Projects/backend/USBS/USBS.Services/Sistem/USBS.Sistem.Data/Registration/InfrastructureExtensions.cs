using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sistem.Application.Contract;
using Sistem.Data.Persistence;
using Sistem.Data.Repositories;

namespace Sistem.Data.Registration
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SistemDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<ISistemConfigRepository, SistemConfigRepository>();
            return services;
        }
    }
}
