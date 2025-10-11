using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using ST.Application.Interfaces.Subscriptions;
using ST.Infrastructure.Services.Subscriptions;
using ST.Application.Common.Behaviors;
using ST.Application.Interfaces.Configuration;
using ST.Domain.Interfaces;
using ST.Infrastructure.Services.Configuration;
using ST.Application.Interfaces.Seeds;
using ST.Infrastructure.Persistence.Contexts;
using ST.Infrastructure.Seeds;
using ST.Domain.Entities.Identity;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Interfaces.Messages;
using ST.Infrastructure.Services.Messages;
using ST.Infrastructure.Services.Email;
using Hangfire;
using Hangfire.PostgreSql;
using ST.Application.Interfaces.Security;
using ST.Infrastructure.Services.Security;
using ST.Application.Interfaces.Common;
using ST.Infrastructure.Services.Common;
using ST.Infrastructure.Tenancy;
using ST.Application.Interfaces.Repositories;
using ST.Infrastructure.Persistence.Repositories;

namespace ST.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Database Configuration
            services.AddDbContext<SharedDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDbContext<TenantDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());

            #endregion

            #region Identity Configuration
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;

            })
            .AddEntityFrameworkStores<SharedDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Error/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });
            #endregion

            #region Core Services
            services.AddHttpContextAccessor();
            services.AddScoped<IFeatureAccessService, FeatureAccessService>();
            services.AddScoped<INotificationService, TempDataNotificationService>();
            services.AddScoped<IEmailSender, DefaultEmailSender>();
            services.AddScoped<MailgunProvider>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddHttpClient();
            services.AddScoped<IProtectedDataService, ProtectedDataService>();
            services.AddScoped<IUrlHelperService, UrlHelperService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ICurrentTenantStore, CurrentTenantStore>();
            #endregion


            #region Authorization Configuration
            services.AddAuthorization();
            #endregion

            #region MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlanCheckPipelineBehavior<,>));
            #endregion

            #region Database Initialization
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<ISeeder, SettingSeeder>();
            services.AddScoped<ISettingSeeder, SettingSeeder>();
            services.AddScoped<IPlanSeeder, PlanSeeder>();
            #endregion

            #region Hangfire
            services.AddHangfire(config => config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));
            services.AddHangfireServer();
            #endregion

            return services;
        }
    }
}
