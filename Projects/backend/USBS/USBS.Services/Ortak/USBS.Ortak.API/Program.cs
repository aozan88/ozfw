using Ortak.API.Validations;
using Ortak.Application.Business;
using USBS.Workers.SampleWorker;
using FluentValidation;
using Ortak.API;
using USBS.Ortak.Application.Mappings;
using USBS.ServiceDefaults;
using Ortak.API.Extensions;


var builder = WebApplication.CreateBuilder(args);

#region Logging Registration
//LoggingServiceExtensions.AddLoggingDefaults(builder);
#endregion

// ConfigureOrtakApi - Exclude
// builder.AddServiceDefaults();

OrtakApiProject.ConfigureOrtakApi(builder);

#region Servis Registration
builder.Services.AddScoped<ISistemTanimServis, SistemTanimServis>();
builder.Services.AddScoped<ISistemTanimTipiServis, SistemTanimTipiServis>();
#endregion

#region Validations Registration
builder.Services.AddValidatorsFromAssembly(typeof(SistemTanimValidation).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(SistemTanimTipiValidation).Assembly);
#endregion
MapsterConfig.RegisterMappings();
// builder.Services.AddRabbitMqEventBusDefaults(builder.Configuration);
/*
builder.Services.AddGraylogLogging(builder.Configuration);
builder.Services.AddTransient<LoggingDelegatingHandler>();
builder.Services.AddHttpClient("MyClient").AddHttpMessageHandler<LoggingDelegatingHandler>();
*/

var app = builder.Build();

OrtakApiProject.ConfigureOrtakApiMiddleware(app);


//adding worker
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<WorkerSettings>(context.Configuration.GetSection("WorkerConfig"));
        services.AddHostedService<MyBackgroundWorker>();
    })
    .Build();

app.Run();
//await host.RunAsync();
