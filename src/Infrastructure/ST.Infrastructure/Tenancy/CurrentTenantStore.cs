using ST.Application.Interfaces.Tenancy;

namespace ST.Infrastructure.Tenancy
{
    public class CurrentTenantStore : ICurrentTenantStore
    {
        public int? Id { get; private set; }

        public void SetTenant(int tenantId)
        {
            Id = tenantId;
        }
    }
}
