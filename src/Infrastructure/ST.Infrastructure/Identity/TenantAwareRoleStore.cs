using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities.Identity;
using ST.Infrastructure.Persistence.Contexts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ST.Infrastructure.Identity
{
    /// <summary>
    /// Standart RoleStore'u, ICurrentTenant servisini kullanarak kiracı bazlı çalışacak şekilde genişletir.
    /// UserManager ve RoleManager'ın tenant'tan haberdar olmadan doğru rolleri bulmasını sağlar.
    /// </summary>
    public class TenantAwareRoleStore : RoleStore<ApplicationRole, SharedDbContext, int, IdentityUserRole<int>, IdentityRoleClaim<int>>
    {
        private readonly ICurrentTenantStore _currentTenant;

        public TenantAwareRoleStore(
            SharedDbContext context,
            ICurrentTenantStore currentTenant,
            IdentityErrorDescriber describer = null) : base(context, describer)
        {
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Rolü ismine göre bulurken sorguya TenantId filtresini otomatik olarak ekler.
        /// Bu metot, hem veritabanını hem de bellekteki (henüz kaydedilmemiş) değişiklikleri kontrol eder.
        /// </summary>
        public override async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Sadece bir tenant context'i varsa filtreleme yap
            if (_currentTenant.Id.HasValue)
            {
                var tenantId = _currentTenant.Id.Value;

                // Önce DbContext'in belleğindeki (ChangeTracker) değişiklikleri ara.
                // Bu, RegisterUserCommandHandler gibi SaveChangesAsync'in henüz çağrılmadığı
                // senaryolarda "TenantId = 0" sorununu çözer.
                var localRole = Context.Roles.Local
                    .FirstOrDefault(r => r.NormalizedName == normalizedRoleName && r.TenantId == tenantId);

                if (localRole != null)
                {
                    return localRole;
                }

                // Bellekte yoksa veritabanında ara.
                return await Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.NormalizedName == normalizedRoleName, cancellationToken);
            }

            // Tenant context'i yoksa (örn: SuperAdmin), TenantId'si null olan rolleri ara.
            return await Roles.FirstOrDefaultAsync(r => r.TenantId == null && r.NormalizedName == normalizedRoleName, cancellationToken);
        }

        // Bu override, _roleManager.Roles sorgusunun en başından filtrelenmesini sağlar.
        public override IQueryable<ApplicationRole> Roles
        {
            get
            {
                return _currentTenant.Id.HasValue
                    ? base.Roles.Where(r => r.TenantId == _currentTenant.Id)
                    : base.Roles.Where(r => r.TenantId == null);
            }
        }
    }
}