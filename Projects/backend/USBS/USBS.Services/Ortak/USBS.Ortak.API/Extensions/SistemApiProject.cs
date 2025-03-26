namespace Ortak.API
{
    public static class OrtakApiProject
    {
        public static void ConfigureOrtakApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            // Custom Scalar logic or services
            builder.Services.AddSingleton<ScalarService>(); // Example Scalar integration service
        }
    }

    public class ScalarService
    {
        public string Process() => "Ortak Scalar Processed!";
    }
}
