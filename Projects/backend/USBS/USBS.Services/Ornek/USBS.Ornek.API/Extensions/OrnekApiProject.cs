using OzFw.ApiBase;
using Serilog;
using USBS.ServiceDefaults;
using Ornek.Application.Registration;
using Ornek.Data.Registration;
using USBS.Ornek.Application.Mappings;

namespace Ornek.API
{
    // Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
    public static class OrnekApiProject
    {
        public static void ConfigureOrnekApi(WebApplicationBuilder builder)
        {
            if (builder.Configuration.GetValue<string>("RunningProfile") != "AppHost")
            {
                LoggingServiceExtensions.AddLoggingDefaults(builder);
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ornek API", Version = "v1" });
            });
            builder.Services.AddOpenApi();
            builder.Services.AddOzFwControllers();
            builder.Services.AddHttpContextAccessor();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanımlı değil.");
            builder.Services.AddOrnekData(connectionString);
            builder.Services.AddOrnekApplication();
            MapsterConfig.RegisterMappings();

            // Olay altyapısı gerekiyorsa: EventBus:Provider = inmemory | rabbitmq | kafka | none
            // builder.Services.AddOzFwEventBus(builder.Configuration);
        }

        public static void ConfigureOrnekApiMiddleware(WebApplication app)
        {
            app.UseSerilogRequestLogging();
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ornek API v1"));
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
