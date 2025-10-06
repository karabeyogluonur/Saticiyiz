using ST.App.Mvc;
using ST.Application.Interfaces.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogServices();
builder.UseSerilog();
builder.Services.AddBaseServices();
builder.Services.AddLayerServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddAuthServices();
builder.Services.AddLogging();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

app.AddDevelopmentBuilder();
app.AddBaseBuilder();
app.AddRouteBuilder();
app.AddAuthBuilder();
app.UseSpecialRoute();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.Run();