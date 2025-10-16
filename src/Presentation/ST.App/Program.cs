using Hangfire;
using ST.App.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogServices();
builder.UseSerilog();
builder.Services.AddHttpContextAccessor();
builder.Services.AddBaseServices();
builder.Services.AddLayerServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddAuthServices();
builder.Services.AddLogging();
builder.Services.AddAutoMapper();
builder.Services.AddFluentValidation();
builder.Services.AddFactoryServices();

WebApplication app = builder.Build();

await app.AddDatabaseInitializerAsync();
app.UseHangfireDashboard();
app.AddDevelopmentBuilder();
app.AddBaseBuilder();
app.AddRouteBuilder();
app.AddAuthBuilder();
app.UseSpecialRoute();
app.UseCustomMiddlewares();
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.Run();
