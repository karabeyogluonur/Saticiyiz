using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ST.Application.Interfaces.Tenancy;
using ST.Infrastructure.Persistence.Contexts;

namespace ST.Infrastructure.Tenancy
{
    public class TenantResolver : ITenantResolver
    {
        private readonly ICurrentTenantStore _currentTenantStore;
        private readonly SharedDbContext _sharedDbContext;
        private readonly IConfiguration _configuration;

        public TenantResolver(
            ICurrentTenantStore currentTenantStore,
            SharedDbContext sharedDbContext,
            IConfiguration configuration)
        {
            _currentTenantStore = currentTenantStore;
            _sharedDbContext = sharedDbContext;
            _configuration = configuration;
        }

        public async Task<string> GetTenantConnectionStringAsync()
        {
            if (_currentTenantStore.Id.HasValue)
            {
                var tenantId = _currentTenantStore.Id.Value;
                var tenant = await _sharedDbContext.ApplicationTenants
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);

                if (tenant != null && tenant.HasDedicatedDatabase)
                {
                    return tenant.ConnectionString;
                }
            }
            return _configuration.GetConnectionString("DefaultConnection");
        }
    }
}