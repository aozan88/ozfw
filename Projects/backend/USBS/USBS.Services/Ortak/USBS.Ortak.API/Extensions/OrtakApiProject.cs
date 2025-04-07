using Ortak.Data.Persistence;
using Ortak.Data.Registration;
using USBS.ServiceDefaults;

namespace Ortak.API
{
    public static class OrtakApiProject
    {
        public static void ConfigureOrtakApi(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            // Add in-memory cache as the distributed cache
            builder.Services.AddDistributedMemoryCache();

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

            builder.Services.AddControllers();

            // Register Scalar Service (or other services)
            //builder.Services.AddSingleton<ScalarService>();

            // Register Database Defaults
            builder.Services.AddDatabaseDefaults(builder.Configuration);

            // Retrieve connection string and register Infrastructure services
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructureServices<OrtakDbContext>(defaultConnectionString);
        }
    }

    public class ScalarService
    {
        public string Process() => "Ortak Scalar Processed!";
    }
}
