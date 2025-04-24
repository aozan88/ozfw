using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Ortak.API;
using Sistem.API;
using USBS.AppHost.Extensions;
using USBS.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
bool useDocker = Environment.GetEnvironmentVariable("USE_DOCKER")?.ToLower() == "true";

// Determine running profile (e.g., AppHost or individual API)
var runningProfile = builder.Configuration.GetValue<string>("RunningProfile") ?? "AppHost";

// Configure service defaults
ServiceDefaultsProject.ConfigureServiceDefaults(builder);

builder.Services.AddDatabaseDefaults(builder.Configuration);

if (runningProfile == "AppHost")
{
    LoggingServiceExtensions.AddLoggingDefaults(builder);
}

// Add specific API project configurations
if (runningProfile == "AppHost" || runningProfile == "Ortak.API")
{
    builder.AddProject("Ortak.API", OrtakApiProject.ConfigureOrtakApi);
}

if (runningProfile == "AppHost" || runningProfile == "Sistem.API")
{
    builder.AddProject("Sistem.API", SistemApiProject.ConfigureSistemApi);
}

// Build application
var app = builder.Build();

OrtakApiProject.ConfigureOrtakApiMiddleware(app);
/*
// Configure middleware
app.UseRouting();
// Example: Add Scalar Middleware (optional)
// app.UseMiddleware<ScalarMiddleware>(); 

// Map API controllers
app.UseEndpoints(endpoints => endpoints.MapControllers());
*/
// Run application
app.Run();
