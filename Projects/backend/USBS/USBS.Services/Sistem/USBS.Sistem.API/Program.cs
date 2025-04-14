using Common.Logging;
using FluentValidation;
using Serilog;
using Sistem.API.Mappings;
using Sistem.API.Validations;
using Sistem.Application.Business;
using Sistem.Application.Mappings;
using Sistem.Data.Persistence;
using Sistem.Data.Registration;
using USBS.Components.Logging;
using USBS.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*
builder.Services.AddDatabaseDefaults(builder.Configuration);
var defaultDbConfig = builder.Services.BuildServiceProvider().GetRequiredService<IDefaultDbContextConfiguration>();
builder.Services.AddInfrastructureServices(defaultDbConfig.ConnectionString);
*/
builder.Host.UseSerilog(SeriLogger.Configure);

builder.AddServiceDefaults();

// Add in-memory cache as the distributed cache
builder.Services.AddDistributedMemoryCache();

// Register Database Defaults
builder.Services.AddDatabaseDefaults(builder.Configuration);

// Retrieve connection string and register Infrastructure services
var defaultConnectionString = builder.Configuration.GetConnectionString("OrtakConnectionString");
builder.Services.AddInfrastructureServices<SistemDbContext>(defaultConnectionString);

builder.Services.AddTransient<LoggingDelegatingHandler>();
// Register Swagger only for Ortak.API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sistem API",
        Version = "v1",
        Description = "API for Sistem data and configurations"
    });
});

builder.Services.AddControllers();

builder.Services.AddScoped<ISistemParametreServis, SistemParametreServis>();

builder.Services.AddValidatorsFromAssembly(typeof(SistemParametreValidation).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAutoMapper(typeof(DtoMappingProfile));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
