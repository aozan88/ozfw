using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
           (context, configuration) =>
           {
               var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
               var connectionString = context.Configuration.GetValue<string>("ConnectionStrings:OrtakConnectionString");
               //var connectionString = context.Configuration.GetConnectionString("OrtakConnectionString");

               var username = context.Configuration.GetValue<string>("ElasticConfiguration:Username");
               var password = context.Configuration.GetValue<string>("ElasticConfiguration:Password");

               configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    //.WriteTo.Debug()
                    .WriteTo.Console()
                    //.WriteTo.File(@"C:\IkysFiles\logs\log-{Date}.log", rollingInterval: RollingInterval.Day)
                    //.WriteTo.MariaDB(connectionString,autoCreateTable:true)
                    //.WriteTo.File("D:\\sil\\log.txt")
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
                        {
                            IndexFormat = "logs", // + context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-") + "-" + context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-") + "-" + DateTime.UtcNow.Date.ToString("dd-MM-yyyy"),
                            AutoRegisterTemplate = true,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1,
                            ModifyConnectionSettings = conn => conn.BasicAuthentication(username, password),
                        })

                    .Enrich.WithClientIp()
                    //.Enrich.WithRequestUserId()
                    .Enrich.WithUserName()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                    //.Enrich.WithUTC3()
                    .ReadFrom.Configuration(context.Configuration);
           };
    }

    public static class UserNameLoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithUserName(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<UserNameEnricher>();
        }
    }

    public static class UtcPropertyLoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithUTC3(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<UtcPropertyEnricher>();
        }
    }

    public class UtcPropertyEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var utcTimestamp = logEvent.Timestamp.ToUniversalTime().AddHours(3);
            //logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Timestamp", utcTimestamp));
        }
    }

    public class UserNameEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserNameEnricher() : this(new HttpContextAccessor())
        {
        }

        public UserNameEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var userName = _contextAccessor.HttpContext?.User?.Identity?.Name;
            var property = propertyFactory.CreateProperty("UserName", userName);

            logEvent.AddOrUpdateProperty(property);
        }
    }
}
