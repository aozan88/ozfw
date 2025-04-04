using Sistem.Data.Registration;
using USBS.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*
builder.Services.AddDatabaseDefaults(builder.Configuration);
var defaultDbConfig = builder.Services.BuildServiceProvider().GetRequiredService<IDefaultDbContextConfiguration>();
builder.Services.AddInfrastructureServices(defaultDbConfig.ConnectionString);
*/


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
