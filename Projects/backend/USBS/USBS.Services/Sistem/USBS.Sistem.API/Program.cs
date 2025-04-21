using Sistem.API.Validations;
using Sistem.Application.Business;
using FluentValidation;
using Sistem.API;
using USBS.ServiceDefaults;
using USBS.Sistem.Application.Mappings;


var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog(SeriLogger.Configure);

// ConfigureSistemApi - Exclude
// builder.AddServiceDefaults();
//LoggingServiceExtensions.AddLoggingDefaults(builder);

SistemApiProject.ConfigureSistemApi(builder);

#region Servis Registration
builder.Services.AddScoped<ISistemParametreServis, SistemParametreServis>();
#endregion

#region Validations Registration
builder.Services.AddValidatorsFromAssembly(typeof(SistemParametreValidation).Assembly);
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

SistemApiProject.ConfigureSistemApiMiddleware(app);

app.Run();
//await host.RunAsync();
