using Sistem.API.Validations;
using Sistem.Application.Business;
using FluentValidation;
using Sistem.Application.Mappings;
using Sistem.API.Mappings;
using Sistem.API;
using USBS.ServiceDefaults;


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
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAutoMapper(typeof(DtoMappingProfile));
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
