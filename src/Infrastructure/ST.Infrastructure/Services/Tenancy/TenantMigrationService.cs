using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Tenancy;
using ST.Infrastructure.Persistence.Contexts;

namespace ST.Infrastructure.Services.Tenancy
{
    public class TenantMigrationService : ITenantMigrationService
    {
        private readonly SharedDbContext _sharedDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantMigrationService> _logger;
        // DbContextOptions'ı doğrudan inject ederek, veritabanı sağlayıcısı (Npgsql) gibi
        // temel ayarları tekrar yazmaktan kurtuluruz.
        private readonly DbContextOptions<TenantDbContext> _tenantDbContextOptions;

        public TenantMigrationService(
            SharedDbContext sharedDbContext,
            IConfiguration configuration,
            ILogger<TenantMigrationService> logger,
            DbContextOptions<TenantDbContext> tenantDbContextOptions) // IDbContextFactory yerine bunu inject ediyoruz
        {
            _sharedDbContext = sharedDbContext;
            _configuration = configuration;
            _logger = logger;
            _tenantDbContextOptions = tenantDbContextOptions;
        }

        public async Task MigrateTenantToDedicatedDatabaseAsync(int tenantId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Başlatılıyor: Tenant {TenantId} için veritabanı taşıma işlemi.", tenantId);

            var tenant = await _sharedDbContext.ApplicationTenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (tenant == null) throw new NotFoundException($"Tenant with ID {tenantId} not found.");
            if (tenant.HasDedicatedDatabase)
            {
                _logger.LogWarning("Tenant {TenantId} zaten özel bir veritabanına sahip. İşlem iptal edildi.", tenantId);
                return;
            }

            var newDbName = $"saticiyiz_tenant_{tenant.Id}_{Guid.NewGuid().ToString("N")[..8]}";
            var newConnectionString = BuildConnectionString(newDbName);

            try
            {
                await CreateDatabaseAsync(newDbName, cancellationToken);
                await ApplyMigrationsAsync(newConnectionString, cancellationToken);
                await TransferDataAsync(tenantId, newConnectionString, cancellationToken);

                tenant.ConnectionString = newConnectionString;
                _sharedDbContext.ApplicationTenants.Update(tenant);
                await _sharedDbContext.SaveChangesAsync(cancellationToken);

                await CleanupOldDataAsync(tenantId, cancellationToken);

                _logger.LogInformation("Başarıyla tamamlandı: Tenant {TenantId} yeni veritabanına taşındı: {DatabaseName}", tenantId, newDbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HATA: Tenant {TenantId} için veritabanı taşıma işlemi başarısız oldu.", tenantId);
                throw;
            }
        }


        private async Task ApplyMigrationsAsync(string connectionString, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Migration'lar uygulanıyor...");

            // 1. Yeni bir DbContextOptionsBuilder oluştur.
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            // 2. Yeni connection string'i ayarla.
            optionsBuilder.UseNpgsql(connectionString);

            // 3. Yeni options ile bir TenantDbContext örneği oluştur.
            await using var context = new TenantDbContext(optionsBuilder.Options);

            await context.Database.MigrateAsync(cancellationToken);
        }

        private async Task TransferDataAsync(int tenantId, string newConnectionString, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Veriler taşınıyor...");
            //var productsToMove = await _sharedDbContext.Products.Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
            //var categoriesToMove = await _sharedDbContext.Categories.Where(c => c.TenantId == tenantId).ToListAsync(cancellationToken);

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(newConnectionString);
            await using var newTenantContext = new TenantDbContext(optionsBuilder.Options);

            //if (categoriesToMove.Any())
            {
                //await newTenantContext.Categories.AddRangeAsync(categoriesToMove, cancellationToken);
            }
            //if (productsToMove.Any())
            {
                //await newTenantContext.Products.AddRangeAsync(productsToMove, cancellationToken);
            }

            await newTenantContext.SaveChangesAsync(cancellationToken);
        }

        // --- Değişiklik Olmayan Metotlar ---

        private string BuildConnectionString(string dbName)
        {
            var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            var builder = new NpgsqlConnectionStringBuilder(defaultConnection) { Database = dbName };
            return builder.ConnectionString;
        }

        private async Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("'{DatabaseName}' veritabanı oluşturuluyor...", dbName);
            var adminConnectionString = BuildConnectionString("postgres");
            await using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync(cancellationToken);
            var commandExists = connection.CreateCommand();
            commandExists.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";
            if (await commandExists.ExecuteScalarAsync(cancellationToken) == null)
            {
                var commandCreate = connection.CreateCommand();
                commandCreate.CommandText = $"CREATE DATABASE \"{dbName}\" TEMPLATE template0";
                await commandCreate.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("'{DatabaseName}' veritabanı zaten mevcut. Oluşturma adımı atlandı.", dbName);
            }
        }

        private async Task CleanupOldDataAsync(int tenantId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Eski veriler temizleniyor...");
            //await _sharedDbContext.Products.Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
            //await _sharedDbContext.Categories.Where(c => c.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        }
    }
}