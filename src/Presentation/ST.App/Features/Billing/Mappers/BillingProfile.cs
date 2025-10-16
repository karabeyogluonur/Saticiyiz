using AutoMapper;
using ST.App.Features.Billing.ViewModels;
using ST.Application.DTOs.Billing;
using ST.Application.Features.Billing.Commands;
using ST.Application.Features.Billing.Commands.CreateBillingProfile;
using ST.Application.Features.Billing.Commands.UpdateBillingProfile;
using ST.Domain.Entities.Billing;

namespace ST.Application.Mappers
{
    public class BillingProfileMappingProfile : Profile
    {
        public BillingProfileMappingProfile()
        {
            CreateMap<BillingProfileAddViewModel, CreateBillingProfileCommand>();
            CreateMap<CreateBillingProfileCommand, BillingProfile>();

            // --- UPDATE ---
            CreateMap<BillingProfileEditViewModel, UpdateBillingProfileCommand>();
            CreateMap<UpdateBillingProfileCommand, BillingProfile>();

            // --- GET (DTO MAPPING) ---

            // Entity'den DTO'ya map'leme (TÜM ALANLARI İÇERECEK ŞEKİLDE GÜNCELLENDİ)
            CreateMap<BillingProfile, BillingProfileDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.Name));
            // Diğer alanlar (Id, Email, Address vb.) isimleri aynı olduğu için
            // AutoMapper tarafından otomatik olarak map'lenecektir.

            // DTO'dan EditViewModel'e map'leme
            CreateMap<BillingProfileDto, BillingProfileEditViewModel>();
        }
    }
}