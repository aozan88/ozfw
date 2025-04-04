using Sistem.Data.Persistence;
using Sistem.Data.Registration;
using USBS.ServiceDefaults;

namespace Sistem.API
{
    public static class SistemApiProject
    {
        public static void ConfigureSistemApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            // Register Scalar Service (or other services)
            builder.Services.AddSingleton<SistemScalarService>();

            // Register Database Defaults
            builder.Services.AddDatabaseDefaults(builder.Configuration);

            // Retrieve connection string and register Infrastructure services
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructureServices<SistemDbContext>(defaultConnectionString);
        }
    }

    public class SistemScalarService
    {
        public string Process() => "Sistem Scalar Processed!";
    }
}
