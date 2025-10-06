using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ST.Infrastructure.Persistence.Contexts;
using ST.Domain.Entities;
using ST.Application.Interfaces.Repositories;

namespace ST.Infrastructure.Tenancy
{
    public class HybridTenantStore : IMultiTenantStore<ApplicationTenant>
    {
        private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
        private readonly IRepository<ApplicationTenant> _tenantRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HybridTenantStore> _logger;

        public HybridTenantStore(
            IUnitOfWork<ApplicationDbContext> unitOfWork,
            IConfiguration configuration,
            ILogger<HybridTenantStore> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantRepository = _unitOfWork.GetRepository<ApplicationTenant>();
            _configuration = configuration;
            _logger = logger;
        }


        public async Task<ApplicationTenant?> TryGetByIdentifierAsync(string identifier)
        {
            var tenant = await _tenantRepository.GetAll()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == identifier);

            if (tenant == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(tenant.ConnectionString))
            {
                tenant.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            }

            return tenant;
        }

        public async Task<ApplicationTenant?> TryGetAsync(string id)
        {
            var tenant = await _tenantRepository.GetAll()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(tenant.ConnectionString))
            {
                tenant.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            }

            return tenant;
        }


        public async Task<bool> TryAddAsync(ApplicationTenant tenant)
        {
            var exists = await _tenantRepository.ExistsAsync(t => t.Identifier == tenant.Identifier);
            if (exists)
            {
                _logger.LogWarning("Attempted to add a tenant with a duplicate identifier: {Identifier}", tenant.Identifier);
                return false;
            }

            await _tenantRepository.InsertAsync(tenant);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding new tenant with identifier: {Identifier}", tenant.Identifier);
                return false;
            }
        }

        public async Task<bool> TryUpdateAsync(ApplicationTenant tenant)
        {
            var existingTenant = await _tenantRepository.GetAll().FirstOrDefaultAsync(t => t.Id == tenant.Id);
            if (existingTenant == null)
            {
                _logger.LogWarning("Attempted to update a non-existent tenant with Id: {Id}", tenant.Id);
                return false;
            }

            _tenantRepository.Update(tenant);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating tenant with Id: {Id}", tenant.Id);
                return false;
            }
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var tenantToRemove = await _tenantRepository.GetAll().FirstOrDefaultAsync(t => t.Identifier == identifier);
            if (tenantToRemove == null)
            {
                _logger.LogWarning("Attempted to remove a non-existent tenant with identifier: {Identifier}", identifier);
                return false;
            }

            tenantToRemove.IsActive = false;
            _tenantRepository.Update(tenantToRemove);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error removing tenant with identifier: {Identifier}", identifier);
                return false;
            }
        }


        public async Task<IEnumerable<ApplicationTenant>> GetAllAsync()
        {
            return await _tenantRepository.GetAll()
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationTenant>> GetAllAsync(int take, int skip)
        {
            return await _tenantRepository.GetAll()
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}