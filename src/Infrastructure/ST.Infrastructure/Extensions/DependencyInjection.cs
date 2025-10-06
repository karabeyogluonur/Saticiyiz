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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
                .AddUnitOfWork<ApplicationDbContext>();

            // 2. Kapsamlı Identity Servis Yapılandırması
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // **A. Şifre Politikaları (Orta Düzey SaaS Standartı)**
                options.Password.RequireDigit = true;         // Rakam gereksin
                options.Password.RequireLowercase = true;     // Küçük harf gereksin
                options.Password.RequireUppercase = true;     // Büyük harf gereksin
                options.Password.RequireNonAlphanumeric = false; // Özel karakter zorunlu değil (çoğu SaaS, karmaşıklığı dengelemek için bunu kaldırır)
                options.Password.RequiredLength = 10;         // Minimum 10 karakter uzunluğu
                options.Password.RequiredUniqueChars = 1;     // Benzersiz karakter gerekliliği

                // **B. Oturum Açma (Sign-In) Ayarları**
                options.SignIn.RequireConfirmedAccount = true;  // ⚠️ E-posta onayını zorunlu kıl (Güvenlik için kritik)
                options.SignIn.RequireConfirmedEmail = true;    // E-posta onayını zorunlu kıl
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // **C. Kullanıcı Kilitleme (Lockout) Ayarları**
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kilitle
                options.Lockout.MaxFailedAccessAttempts = 5;                     // 5 başarısız denemeden sonra kilitle
                options.Lockout.AllowedForNewUsers = true;

                // **D. Kullanıcı Ayarları**
                options.User.RequireUniqueEmail = true;         // E-posta benzersiz olmalı

            })
            .AddEntityFrameworkStores<ApplicationDbContext>() // Identity bilgilerini DbContext'te tut
            .AddDefaultTokenProviders();                    // Şifre sıfırlama vb. için token sağlayıcıları ekle

            // 3. HttpContext ve Servis Bağımlılıkları
            services.AddHttpContextAccessor();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFeatureAccessService, FeatureAccessService>();

            // 4. Multi-Tenancy (Finbuckle) Yapılandırması
            services.AddMultiTenant<ApplicationTenant>()
                .WithStore<HybridTenantStore>(ServiceLifetime.Scoped)
                .WithDelegateStrategy(context =>
                {
                    string? tenantId = (context as HttpContext)?.User.FindFirstValue(CustomClaims.TenantId);
                    return Task.FromResult(tenantId);
                });

            // 5. Yetkilendirme (Authorization) Yapılandırması
            services.AddScoped<IAuthorizationHandler, TenantMemberHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustBeMemberOfTenant", policy =>
                    policy.AddRequirements(new TenantMemberRequirement()));
            });

            // 6. MediatR Pipeline Davranışları
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlanCheckPipelineBehavior<,>));

            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<ISeeder, SettingSeeder>();
            services.AddScoped<ISettingSeeder, SettingSeeder>();


            return services;
        }
    }
}