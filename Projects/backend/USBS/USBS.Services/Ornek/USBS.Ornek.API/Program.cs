using Ornek.API;

// Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
var builder = WebApplication.CreateBuilder(args);
// OpenTelemetry, health check, service discovery, resilience (USBS.ServiceDefaults)
builder.AddServiceDefaults();

OrnekApiProject.ConfigureOrnekApi(builder);

var app = builder.Build();
app.MapDefaultEndpoints();

OrnekApiProject.ConfigureOrnekApiMiddleware(app);

app.Run();
