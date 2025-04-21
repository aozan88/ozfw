using Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using USBS.Components.Logging;

namespace Ortak.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            //services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly); // FluentValidation
            return services;
        }
        public static void ConfigureBaseLogging(WebApplicationBuilder builder)
        {
            // Setup Serilog base configuration
            builder.Host.UseSerilog(SeriLogger.Configure);

            // Add LoggingDelegatingHandler for HTTP request logging
            builder.Services.AddTransient<LoggingDelegatingHandler>();
            builder.Services.AddHttpClient("DefaultClient")


                .AddHttpMessageHandler<LoggingDelegatingHandler>();
        }
       }
}
