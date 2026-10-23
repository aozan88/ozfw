using Sistem.API.Validations;
using Sistem.Application.Business;
using FluentValidation;
using Sistem.API;
using USBS.Sistem.Application.Mappings;


var builder = WebApplication.CreateBuilder(args);
// OpenTelemetry, health check, service discovery, resilience (USBS.ServiceDefaults)
builder.AddServiceDefaults();
bool useDocker = Environment.GetEnvironmentVariable("USE_DOCKER")?.ToLower() == "true";
//builder.Host.UseSerilog(SeriLogger.Configure);

var runningProfile = builder.Configuration.GetValue<string>("RunningProfile") ?? "AppHost";

SistemApiProject.ConfigureSistemApi(builder);

#region Servis Registration
builder.Services.AddScoped<ISistemParametreServis, SistemParametreServis>();
#endregion

#region Validations Registration
builder.Services.AddValidatorsFromAssembly(typeof(SistemParametreValidation).Assembly);
// Application katmanındaki DTO validator'ları (BaseController Payload doğrulamasında kullanır)
builder.Services.AddValidatorsFromAssembly(typeof(Sistem.Application.Validations.SistemParametreDtoValidation).Assembly);
#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
#region Mapping Registration
MapsterConfig.RegisterMappings();
#endregion

// builder.Services.AddRabbitMqEventBusDefaults(builder.Configuration);
/*
builder.Services.AddGraylogLogging(builder.Configuration);
builder.Services.AddTransient<LoggingDelegatingHandler>();
builder.Services.AddHttpClient("MyClient").AddHttpMessageHandler<LoggingDelegatingHandler>();
*/
var app = builder.Build();
app.MapDefaultEndpoints();

SistemApiProject.ConfigureSistemApiMiddleware(app);

app.Run();
//await host.RunAsync();
