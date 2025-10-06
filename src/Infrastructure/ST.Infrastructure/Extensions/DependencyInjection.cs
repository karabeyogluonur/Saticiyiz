using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Finbuckle.MultiTenant;
using System.Security.Claims;
using ST.Infrastructure.Identity;
using ST.Application.Interfaces.Identity;
using ST.Infrastructure.Services.Identity;
using ST.Application.Interfaces.Subscriptions;
using ST.Infrastructure.Services.Subscriptions;
using ST.Infrastructure.Tenancy;
using ST.Infrastructure.Security.Authorization;
using ST.Application.Common.Authorization;
using ST.Application.Common.Behaviors;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Configuration;
using ST.Domain.Interfaces;
using ST.Infrastructure.Services.Configuration;
using ST.Application.Interfaces.Seeds;
using ST.Infrastructure.Persistence.Contexts;
using ST.Infrastructure.Seeds;
using ST.Domain.Entities;

namespace ST.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Database Configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
                .AddUnitOfWork<ApplicationDbContext>();
            #endregion

            #region Identity Configuration
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;

            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            #endregion

            #region Core Services
            services.AddHttpContextAccessor();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFeatureAccessService, FeatureAccessService>();
            #endregion

            #region Multi-Tenancy Configuration
            services.AddMultiTenant<ApplicationTenant>()
                .WithStore<HybridTenantStore>(ServiceLifetime.Scoped)
                .WithDelegateStrategy(context =>
                {
                    string? tenantId = (context as HttpContext)?.User.FindFirstValue(CustomClaims.TenantId);
                    return Task.FromResult(tenantId);
                });
            #endregion

            #region Authorization Configuration
            services.AddScoped<IAuthorizationHandler, TenantMemberHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustBeMemberOfTenant", policy =>
                    policy.AddRequirements(new TenantMemberRequirement()));
            });
            #endregion

            #region MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlanCheckPipelineBehavior<,>));
            #endregion

            #region Database Initialization
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<ISeeder, SettingSeeder>();
            services.AddScoped<ISettingSeeder, SettingSeeder>();
            #endregion

            return services;
        }
    }
}