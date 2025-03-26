using Microsoft.Extensions.DependencyInjection;

namespace Ortak.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            //services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly); // FluentValidation
            return services;
        }
    }
}
