namespace ST.Application.Interfaces.Tenancy
{
    /// <summary>
    /// Bir kiracının verilerini ortak veritabanından kendi özel veritabanına taşır.
    /// </summary>
    public interface ITenantMigrationService
    {
        /// <summary>
        /// Belirtilen kiracı için yeni bir veritabanı oluşturur, migration'ları çalıştırır ve verileri taşır.
        /// </summary>
        /// <param name="tenantId">Taşınacak kiracının kimliği.</param>
        /// <param name="cancellationToken">İptal token'ı.</param>
        Task MigrateTenantToDedicatedDatabaseAsync(int tenantId, CancellationToken cancellationToken);
    }
}