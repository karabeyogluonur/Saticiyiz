using MediatR;
using ST.Application.Wrappers;
using ST.Domain.Enums;

namespace ST.Application.Features.Billing.Commands.UpdateBillingProfile
{
    public class UpdateBillingProfileCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public BillingAccountType BillingAccountType { get; set; }
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = default!;
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public string PostalCode { get; set; } = default!;
        public string? IndividualFirstName { get; set; }
        public string? IndividualLastName { get; set; }
        public string? IndividualIdentityNumber { get; set; }
        public string? CorporateCompanyName { get; set; }
        public string? CorporateTaxOffice { get; set; }
        public string? CorporateTaxNumber { get; set; }
    }
}