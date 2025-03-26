using KonFw.BaseInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace USBS.Components.DbManagement
{
    public static class DbManagementExtensions
    {
        public static IServiceCollection AddDbManagement<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDbContext)
            where TContext : KonFwDbContext
        {
            services.AddDbContext<TContext>(configureDbContext);
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
