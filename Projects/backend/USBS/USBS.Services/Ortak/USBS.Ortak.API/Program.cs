using Ortak.Data.Registration;
using USBS.Components.Logging;


var builder = WebApplication.CreateBuilder(args);


//builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));

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
