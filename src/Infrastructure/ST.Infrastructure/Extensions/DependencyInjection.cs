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
using ST.Infrastructure.Services.Tenancy;
using ST.Application.Interfaces.Contexts;
using ST.Application.Interfaces.Identity;
using ST.Infrastructure.Services.Identity;
using ST.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ST.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            #region Database Configuration

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // --- DbContext KAYITLARI (Nihai Hali) ---

            // 1. SharedDbContext: Her zaman varsayılan bağlantıyı kullanır.
            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                throw new InvalidOperationException("DefaultConnection is not configured.");
            }
            services.AddDbContext<SharedDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            // 2. TenantDbContext:
            // Bu kayıt, 'dotnet ef migrations add' komutunun çalışması için gereklidir.
            // Komut çalışırken, bir veritabanı sağlayıcısı belirtilmiş olmalıdır.
            // Bu yüzden ona da geçici olarak default connection string'i veriyoruz.
            // Runtime'da bu bağlantı, aşağıdaki AddScoped<ITenantDbContext> kaydı sayesinde ezilecektir.
            services.AddDbContext<TenantDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            // 3. ISharedDbContext ve ITenantDbContext arayüzlerini somut sınıflara bağlıyoruz.
            // Bu, UnitOfWork'ün doğru context'leri almasını sağlar.
            services.AddScoped<ISharedDbContext>(provider => provider.GetRequiredService<SharedDbContext>());

            // Bu, dinamik bağlantının anahtarıdır. Her istekte ITenantResolver'ı kullanarak
            // doğru connection string ile yeni bir TenantDbContext örneği oluşturur.
            services.AddScoped<ITenantDbContext>(serviceProvider =>
            {
                var resolver = serviceProvider.GetRequiredService<ITenantResolver>();
                var connectionString = resolver.GetTenantConnectionStringAsync().GetAwaiter().GetResult();

                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                return new TenantDbContext(optionsBuilder.Options);
            });

            #endregion

            #region Identity Configuration
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;

            })
            .AddEntityFrameworkStores<SharedDbContext>()
            .AddUserStore<TenantAwareUserStore>()
            .AddRoleStore<TenantAwareRoleStore>()
            .AddDefaultTokenProviders()
            .AddRoleValidator<TenantRoleValidator<ApplicationRole>>();

            services.AddScoped<UserManager<ApplicationUser>, TenantAwareUserManager>();
            services.AddScoped<IUserStore<ApplicationUser>, TenantAwareUserStore>();
            services.AddScoped<IRoleStore<ApplicationRole>, TenantAwareRoleStore>();




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
            services.AddScoped<ITenantResolver, TenantResolver>();
            services.AddScoped<ITenantMigrationService, TenantMigrationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IRoleService, RoleService>();


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
