using Microsoft.AspNetCore.Builder;

namespace USBS.AppHost.Extensions
{
    public static class ProjectExtensions
    {
        public static WebApplicationBuilder AddProject(this WebApplicationBuilder builder, string projectName, Action<WebApplicationBuilder> configureProject)
        {
            Console.WriteLine($"Initializing project: {projectName}");
            configureProject(builder);
            return builder;
        }
    }
}
