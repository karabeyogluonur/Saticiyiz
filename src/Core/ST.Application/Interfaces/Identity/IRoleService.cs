using System.Threading.Tasks;

namespace ST.Application.Interfaces.Identity
{
    public interface IRoleService
    {
        Task CreateDefaultRolesForTenantAsync(string tenantId);
    }
}