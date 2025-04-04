using Microsoft.AspNetCore.Builder;
using Ortak.API;
using Sistem.API;
using USBS.AppHost.Extensions;
using USBS.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// builder.AddProject<Projects.USBS_ServiceDefaults>("servicedefaults");
// builder.Services.AddKafkaEventBusDefaults(builder.Configuration);

ServiceDefaultsProject.ConfigureServiceDefaults(builder);

// Add database configuration
// var kafka = builder.Services.AddKafkaEventBusDefaults(kafkaConfigSection);
// var rabbit = builder.Services.AddRabbitMqEventBusDefaults(rabbitConfigSection);
// var redis = builder.Services.AddRedisCacheDefaults(redisConfigSection);
// var postgre = builder.Services.AddDatabaseDefaults(redisConfigSection);

//var apiService = builder.AddProject<Projects.USBS_ApiService>("apiservice");
//var serviceDefaults = builder.AddProject<Projects.USBS_ServiceDefaults>("apiservice");
/*
builder.AddProject<Projects.USBS_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);
*/

// Add Ortak.API project
// builder.AddProject("ServiceDefaults", ServiceDefaultsProject.ConfigureServiceDefaults);

builder.Services.AddDatabaseDefaults(builder.Configuration);

// Add Ortak.API project
builder.AddProject("Ortak.API", OrtakApiProject.ConfigureOrtakApi);

// Add Sistem.API project
builder.AddProject("Sistem.API", SistemApiProject.ConfigureSistemApi);

var app = builder.Build();

// Use Scalar middleware instead of GraphQL
app.UseRouting();
app.UseMiddleware<ScalarMiddleware>(); // Example of Scalar Middleware addition
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // API controllers mapping if necessary
});

app.Run();