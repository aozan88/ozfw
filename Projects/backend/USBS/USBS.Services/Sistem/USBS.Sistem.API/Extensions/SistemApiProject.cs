using OzFw.ApiBase;
using Serilog;
using Sistem.Data.Persistence;
using Sistem.Data.Registration;
using USBS.ServiceDefaults;

namespace Sistem.API
{
    public static class SistemApiProject
    {
        public static void ConfigureSistemApi(WebApplicationBuilder builder)
        {
            // Add in-memory cache as the distributed cache
            if (builder.Configuration.GetValue<string>("RunningProfile") != "AppHost")
            {
                LoggingServiceExtensions.AddLoggingDefaults(builder);
            }
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddEndpointsApiExplorer();
            // Register Swagger only for Ortak.API
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Sistem API",
                    Version = "v1",
                    Description = "API for Sistem data and configurations"
                });
            });

            builder.Services.AddOpenApi();
            builder.Services.AddOzFwControllers();
            builder.Services.AddHttpContextAccessor();


            //Register rabbitmq and kafka
            // EventBus:Provider = inmemory (varsayılan) | rabbitmq | kafka | none
            builder.Services.AddOzFwEventBus(builder.Configuration);

            // Register Database Defaults
            builder.Services.AddDatabaseDefaults(builder.Configuration);

            // Retrieve connection string and register Infrastructure services
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructureServices<SistemDbContext>(defaultConnectionString);
        }


        public static void ConfigureSistemApiMiddleware(WebApplication app)
        {
            // Add middleware specific to Ortak.API
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistem API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
