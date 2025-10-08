using System.Reflection;
using FluentValidation;
using Serilog;
using ST.Application.Extensions;
using ST.Application.Interfaces.Identity;
using ST.Infrastructure.Extensions;

namespace ST.App.Mvc
{
    public static class ServiceRegistration
    {
        public static void AddBaseServices(this IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddMemoryCache();
        }
        public static void AddSerilogServices(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
        }
        public static void AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }
        public static void AddFluentValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public static void AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }

        public static void AddLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationServices();
            services.AddInfrastructureServices(configuration);
        }
    }
}