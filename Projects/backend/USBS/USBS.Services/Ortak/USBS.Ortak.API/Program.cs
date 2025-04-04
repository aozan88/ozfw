using Ortak.Data.Registration;
using USBS.Components.Logging;
using USBS.ServiceDefaults;
using USBS.Workers.SampleWorker;

var builder = WebApplication.CreateBuilder(args);


//builder.AddServiceDefaults();

// Add services to the container.
/*
builder.Services.AddDatabaseDefaults(builder.Configuration);
var defaultDbConfig = builder.Services.BuildServiceProvider().GetRequiredService<IDefaultDbContextConfiguration>();
builder.Services.AddInfrastructureServices(defaultDbConfig.ConnectionString);
// builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));
*/

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();


builder.Services.AddHttpClient("MyClient").AddHttpMessageHandler<LoggingDelegatingHandler>();

builder.Services.AddLogging();

var app = builder.Build();

// app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
