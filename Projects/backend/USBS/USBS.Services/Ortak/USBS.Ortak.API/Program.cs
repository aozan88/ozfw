using Ortak.Data.Registration;
using Ortak.API.Extensions;

var builder = WebApplication.CreateBuilder(args);


//builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
