using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Wrappers;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Enums;
using ST.Domain.Events.Tenancy;
using ST.Infrastructure.Persistence.Contexts;

namespace ST.Infrastructure.Services.Tenancy
{
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork<ApplicationDbContext> _applicationUnitOfWork;
        private readonly IRepository<ApplicationTenant> _applicationTenantRepository;

        public TenantService(IUnitOfWork<ApplicationDbContext> applicationUnitOfWork)
        {
            _applicationUnitOfWork = applicationUnitOfWork;
            _applicationTenantRepository = _applicationUnitOfWork.GetRepository<ApplicationTenant>();
        }

        public async Task<Response<ApplicationTenant>> CreateTenantAsync()
        {
            string tenantName = Guid.NewGuid().ToString();
            ApplicationTenant applicationTenant = new ApplicationTenant
            {
                Id = tenantName,
                ConnectionString = tenantName,
                Identifier = tenantName,
                CreatedBy = "system",
                Name = tenantName,
            };

            await _applicationTenantRepository.InsertAsync(applicationTenant);

            applicationTenant.AddDomainEvent(new TenantCreatedEvent(applicationTenant.Id, applicationTenant.Name));

            return Response<ApplicationTenant>.Success(applicationTenant, "Tenant başarıyla oluşturuldu.");
        }
    }
}