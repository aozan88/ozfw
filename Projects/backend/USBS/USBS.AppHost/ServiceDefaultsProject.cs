using Microsoft.AspNetCore.Builder;
using USBS.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Logging;
using Serilog;
using USBS.Components.Logging;

public static class ServiceDefaultsProject
{
    public static void ConfigureServiceDefaults(WebApplicationBuilder builder)
    {
        try
        {
            builder.Services.AddHttpContextAccessor();

            // Configure base logging (Serilog configuration)
            ConfigureBaseLogging(builder);

            var kafkaConfigSection = builder.Configuration.GetSection("KafkaEventBus");
            var rabbitConfigSection = builder.Configuration.GetSection("RabbitMqEventBus");
            var redisConfigSection = builder.Configuration.GetSection("RedisCache");
            var postgreConfigSection = builder.Configuration.GetSection("ConnectionStrings");
            var dbConnectionString = builder.Configuration.GetConnectionString("OrtakConnectionString");
            // builder.Services.AddKafkaEventBusDefaults(config => kafkaConfigSection.Bind(config));
            var kafka = builder.Services.AddKafkaEventBusDefaults(builder.Configuration);
            var rabbit = builder.Services.AddRabbitMqEventBusDefaults(builder.Configuration);
            var redis = builder.Services.AddRedisCacheDefaults(builder.Configuration);
            //var postgre = builder.Services.AddDatabaseDefaults(postgreConfigSection);
            Console.WriteLine("[ServiceDefaults] Kafka Event Bus configured.");

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception while registering ServiceDefaults");
        }
    }

    private static void ConfigureBaseLogging(WebApplicationBuilder builder)
    {
        // Setup Serilog base configuration
        builder.Host.UseSerilog(SeriLogger.Configure);

        // Add LoggingDelegatingHandler for HTTP request logging
        builder.Services.AddTransient<LoggingDelegatingHandler>();
        builder.Services.AddHttpClient("DefaultClient")
            .AddHttpMessageHandler<LoggingDelegatingHandler>();
    }
}
