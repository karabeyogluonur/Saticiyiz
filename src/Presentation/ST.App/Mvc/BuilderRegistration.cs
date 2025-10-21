using Serilog;
using Serilog.Sinks.PostgreSQL;
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

    public static void UseSerilog(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        var columnOptions = new Dictionary<string, ColumnWriterBase>
        {
            { "message", new RenderedMessageColumnWriter() },
            { "message_template", new MessageTemplateColumnWriter() },
            { "level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter() },
            { "exception", new ExceptionColumnWriter() },
            { "properties", new LogEventSerializedColumnWriter() },
            { "props_test", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Jsonb) },
            { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Varchar, "l") },
            { "user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Varchar) },
            { "tenant_id", new SinglePropertyColumnWriter("TenantId", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Varchar) },
            { "request_path", new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Varchar) }
        };

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(
                connectionString,
                "Logs",
                columnOptions,
                needAutoCreateTable: true
            )
        );
    }
    public static void UseCustomMiddlewares(this WebApplication builder)
    {
        builder.UseMiddleware<LogContextMiddleware>();
        builder.UseMiddleware<TenantResolverMiddleware>();
        builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
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
