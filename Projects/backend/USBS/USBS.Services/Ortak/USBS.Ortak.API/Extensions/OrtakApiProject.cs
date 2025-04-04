using Ortak.Data.Persistence;
using Ortak.Data.Registration;
using USBS.ServiceDefaults;

namespace Ortak.API
{
    public static class OrtakApiProject
    {
        public static void ConfigureOrtakApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            // Register Scalar Service (or other services)
            builder.Services.AddSingleton<ScalarService>();

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
