using MediatR;
using ST.Application.DTOs.Billing; // Önceki adımda oluşturmuştuk
using ST.Application.Wrappers;

namespace ST.Application.Features.Billing.Queries.GetBillingProfileById
{
    // Düzenleme sayfası için veri getirme sorgusu
    public class GetBillingProfileByIdQuery : IRequest<Response<BillingProfileDto>>
    {
        public int Id { get; set; }
    }
}