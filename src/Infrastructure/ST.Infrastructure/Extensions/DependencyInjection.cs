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
using ST.Application.Interfaces.Billing;
using ST.Infrastructure.Services.Billing;

namespace ST.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            #region Database Configuration

            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                throw new InvalidOperationException("DefaultConnection 'appsettings.json' dosyasında yapılandırılmamış.");
            }

            // 2. SharedDbContext'i standart ve doğru yöntemle kaydet.
            // AddDbContext, DbContext'i Scoped olarak kaydeder. DI konteyneri,
            // yine Scoped olarak kayıtlı olan ICurrentTenantStore'u her istekte
            // SharedDbContext'in constructor'ına otomatik olarak enjekte edecektir.
            services.AddDbContext<SharedDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            // 3. TenantDbContext:
            // Bu kayıt, 'dotnet ef migrations add' komutunun tasarım zamanında (design-time)
            // çalışabilmesi için gereklidir. Runtime'da aşağıdaki fabrika metodu tarafından ezilecektir.
            services.AddDbContext<TenantDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            // 4. Arayüzleri somut sınıflara bağla.
            // ISharedDbContext istendiğinde, o istek için oluşturulmuş SharedDbContext örneğini ver.
            services.AddScoped<ISharedDbContext>(provider => provider.GetRequiredService<SharedDbContext>());

            // 5. ITenantDbContext'i dinamik olarak oluşturan fabrika metodu.
            // Her istekte, o anki kiracının veritabanı bağlantı bilgisini kullanarak
            // TenantDbContext'i oluşturur. Bu kısım doğru ve kalmalıdır.
            services.AddScoped<ITenantDbContext>(serviceProvider =>
            {
                var resolver = serviceProvider.GetRequiredService<ITenantResolver>();
                var connectionString = resolver.GetTenantConnectionStringAsync().GetAwaiter().GetResult();

                // Tenant'a özel bir veritabanı bağlantısı bulunamazsa hata fırlatmak yerine
                // null bir context döndürmek veya varsayılan bir davranış belirlemek daha güvenli olabilir.
                // Şimdilik mevcut mantığı koruyoruz.
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Veya loglama yapıp null dönebilirsiniz.
                    throw new InvalidOperationException("Mevcut kiracı için veritabanı bağlantısı bulunamadı.");
                }

                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                // TenantDbContext'in constructor'ı parametresiz ise bu satır yeterli.
                return new TenantDbContext(optionsBuilder.Options);
            });

            // UnitOfWork kaydını da standart olarak ekliyoruz.
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

            var cookieSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = $".Saticiyiz.Auth_{cookieSuffix}";
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
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IBillingProfileService, BillingProfileService>();
            services.AddScoped<IHttpContextUserClaimService, HttpContextUserClaimService>();


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
