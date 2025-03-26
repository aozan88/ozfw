namespace Sistem.API
{
    public static class SistemApiProject
    {
        public static void ConfigureSistemApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            // Custom Scalar logic or services
            builder.Services.AddSingleton<SistemScalarService>(); // Example Scalar integration service
        }
    }

    public class SistemScalarService
    {
        public string Process() => "Sistem Scalar Processed!";
    }
}
