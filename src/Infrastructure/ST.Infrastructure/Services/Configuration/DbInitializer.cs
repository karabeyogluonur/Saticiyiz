using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Contexts;
using ST.Application.Interfaces.Seeds;

namespace ST.Infrastructure.Services.Configuration
{
    internal class DbInitializer : IDbInitializer
    {
        private readonly ISharedDbContext _sharedDbContext;
        private readonly IPlanSeeder _planSeeder;
        private readonly ISettingSeeder _settingSeeder;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(
            ISharedDbContext sharedDbContext,
            ILogger<DbInitializer> logger,
            IPlanSeeder planSeeder,
            ISettingSeeder settingSeeder)
        {
            _sharedDbContext = sharedDbContext;
            _logger = logger;
            _planSeeder = planSeeder;
            _settingSeeder = settingSeeder;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // 1. Veritabanı Migration'larını Uygula
                if (_sharedDbContext.Database.GetPendingMigrations().Any())
                {
                    _logger.LogInformation("Shared Database migration'ları uygulanıyor...");
                    await _sharedDbContext.Database.MigrateAsync();
                    _logger.LogInformation("Shared Database migration'ları başarıyla uygulandı.");
                }
                else
                {
                    _logger.LogInformation("Shared Database için bekleyen migration bulunmuyor.");
                }

                await _settingSeeder.SeedAsync();
                await _planSeeder.SeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı başlatma ve seed etme sırasında bir hata oluştu.");
                throw;
            }
        }
    }
}