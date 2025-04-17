using Ortak.Data.Persistence;
using Ortak.Data.Registration;
using Serilog;
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
            builder.Services.AddControllers();

            // Register Database Defaults
            builder.Services.AddDatabaseDefaults(builder.Configuration);

            // Retrieve connection string and register Infrastructure services
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructureServices<OrtakDbContext>(defaultConnectionString);
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
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
