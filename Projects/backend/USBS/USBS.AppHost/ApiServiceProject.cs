using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class ApiServiceProject
{
    public static void ConfigureApiService(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen();
    }
}
