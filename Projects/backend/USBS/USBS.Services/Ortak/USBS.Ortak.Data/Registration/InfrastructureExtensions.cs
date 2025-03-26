using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ortak.Application.Contract;
using Ortak.Data.Persistence;
using Ortak.Data.Repositories;

namespace Ortak.Data.Registration
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<OrtakDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IKodRepository, KodRepository>();
            return services;
        }
    }
}
