using Serilog;
using ST.App.Mvc.Middlewares;
using ST.Application.Interfaces.Configuration;

namespace ST.App.Mvc;

public static class BuilderRegistration
{
    public static void AddBaseBuilder(this WebApplication application)
    {
        application.UseHttpsRedirection();
        application.UseStaticFiles();
    }
    public static void UseSerilog(this WebApplicationBuilder? builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
    }
    public static void UseCustomMiddlewares(this WebApplication builder)
    {
        builder.UseMiddleware<SetupMiddleware>();
    }
    public static async Task AddDatabaseInitializerAsync(this WebApplication builder)
    {
        using (IServiceScope scope = builder.Services.CreateScope())
        {
            IDbInitializer initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            await initializer.InitializeAsync();
        }
    }
    public static void AddDevelopmentBuilder(this WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            application.UseDeveloperExceptionPage();
            application.UseMigrationsEndPoint();
        }
        else
        {
            application.UseExceptionHandler("/Error");
            application.UseHsts();
        }
    }
    public static void AddRouteBuilder(this WebApplication app)
    {
        app.UseRouting();
    }
    public static void AddAuthBuilder(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
    public static void UseSpecialRoute(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}

