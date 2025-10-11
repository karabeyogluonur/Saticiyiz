
using System.ComponentModel.DataAnnotations.Schema;
using ST.Domain.Entities.Common;
using ST.Domain.Entities.Lookup;
using ST.Domain.Enums;
using ST.Domain.Events.Common;

namespace ST.Domain.Entities.Billing;

public class BillingProfile : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public virtual ApplicationTenant Tenant { get; set; }
    public BillingAccountType BillingAccountType { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Address { get; set; }
    public int CityId { get; set; }
    public virtual City City { get; set; }
    public int DistrictId { get; set; }
    public virtual District District { get; set; }
    public string PostalCode { get; set; }

    public string? IndividualFirstName { get; set; }
    public string? IndividualLastName { get; set; }
    public string? IndividualIdentityNumber { get; set; }

    public string? CorporateCompanyName { get; set; }
    public string? CorporateTaxOffice { get; set; }
    public string? CorporateTaxNumber { get; set; }

    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
