using Ortak.API.Validations;
using Ortak.Application.Business;
using Ortak.Data.Persistence;
using Ortak.Data.Registration;
using USBS.Components.Logging;
using USBS.ServiceDefaults;
using USBS.Workers.SampleWorker;
using FluentValidation;
using Ortak.Application.Mappings;
using Ortak.API.Mappings;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();

builder.Services.AddDistributedMemoryCache();

// Add services to the container.
/*
builder.Services.AddDatabaseDefaults(builder.Configuration);
var defaultDbConfig = builder.Services.BuildServiceProvider().GetRequiredService<IDefaultDbContextConfiguration>();
builder.Services.AddInfrastructureServices(defaultDbConfig.ConnectionString);
// builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));
*/
// Register Swagger only for Ortak.API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ortak API",
        Version = "v1",
        Description = "API for Ortak data and configurations"
    });
});


builder.Services.AddControllers();

builder.Services.AddScoped<ISistemTanimServis, SistemTanimServis>();
builder.Services.AddScoped<ISistemTanimTipiServis, SistemTanimTipiServis>();

builder.Services.AddValidatorsFromAssembly(typeof(SistemTanimValidation).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(SistemTanimTipiValidation).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAutoMapper(typeof(DtoMappingProfile));

// Register Database Defaults
builder.Services.AddDatabaseDefaults(builder.Configuration);

// Retrieve connection string and register Infrastructure services
var defaultConnectionString = builder.Configuration.GetConnectionString("OrtakConnectionString");
builder.Services.AddInfrastructureServices<OrtakDbContext>(defaultConnectionString);

builder.Services.AddHttpClient("MyClient").AddHttpMessageHandler<LoggingDelegatingHandler>();

builder.Services.AddLogging();

var app = builder.Build();

// app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ortak API v1");
    });
}

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();

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

await host.RunAsync();
