using ST.Application.Interfaces.Tenancy;

namespace ST.Infrastructure.Jobs
{
    public class TenantMigrationJob
    {
        private readonly ITenantMigrationService _migrationService;

        public TenantMigrationJob(ITenantMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        // Hangfire bu metodu çağıracak
        public async Task RunAsync(int tenantId)
        {
            await _migrationService.MigrateTenantToDedicatedDatabaseAsync(tenantId, CancellationToken.None);
        }
    }
}