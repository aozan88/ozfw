using Ornek.API;

// Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
var builder = WebApplication.CreateBuilder(args);

OrnekApiProject.ConfigureOrnekApi(builder);

var app = builder.Build();

OrnekApiProject.ConfigureOrnekApiMiddleware(app);

app.Run();
