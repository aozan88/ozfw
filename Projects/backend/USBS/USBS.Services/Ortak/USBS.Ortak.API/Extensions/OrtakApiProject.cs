using OzFw.ApiBase;
using OzFwEventBus;
using Ortak.Data.Persistence;
using Ortak.Data.Registration;
using Serilog;
using USBS.Ortak.API.Consumers;
using USBS.ServiceDefaults;

namespace Ortak.API
{
    public static class OrtakApiProject
    {
        public static void ConfigureOrtakApi(WebApplicationBuilder builder)
        {
            //builder.Services.AddHttpContextAccessor();
            if (builder.Configuration.GetValue<string>("RunningProfile") != "AppHost")
            {
                LoggingServiceExtensions.AddLoggingDefaults(builder);
            }

            // Add in-memory cache as the distributed cache
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Register Swagger only for Ortak.API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Ortak API",
                    Version = "v1",
                    Description = "API for Ortak data and configurations"
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
            builder.Services.AddInfrastructureServices<OrtakDbContext>(defaultConnectionString);

            builder.Services.AddScoped<IEventHandler<SistemTanimTipiSilindiEvent>, SistemTanimTipiSilindiEventHandler>();
            builder.Services.AddScoped<SistemTanimTipiSilindiEventHandler>();
        }

        /// <summary>Ortak modülünün olay abonelikleri (önceden USBS.ServiceDefaults içindeydi).</summary>
        public static void UseOrtakEventSubscriptions(WebApplication app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            try
            {
                eventBus.SubscribeAsync<SistemTanimTipiSilindiEvent, SistemTanimTipiSilindiEventHandler>().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Broker erişilemezse API yine de ayağa kalksın; olaylar işlenmez
                app.Logger.LogError(ex, "Olay aboneliği başarısız ({EventBus})", eventBus.GetType().Name);
            }
        }

        public static void ConfigureOrtakApiMiddleware(WebApplication app)
        {
            // Add middleware specific to Ortak.API
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ortak API v1");
                    c.RoutePrefix = "swagger";
                });
                
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllers();

            UseOrtakEventSubscriptions(app);
        }
    }
}
