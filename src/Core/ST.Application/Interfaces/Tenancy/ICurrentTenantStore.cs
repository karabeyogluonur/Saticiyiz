namespace ST.Application.Interfaces.Tenancy
{
    public interface ICurrentTenantStore
    {
        int? Id { get; }
        void SetTenant(int tenantId);
    }
}
