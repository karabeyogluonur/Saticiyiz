using AutoMapper;
using ST.Application.Features.Identity.Commands.RegisterUser;

using ST.Domain.Entities.Identity;

namespace ST.Application.Mappers
{
    public class IdentityProfile : Profile
    {
        public IdentityProfile()
        {
            CreateMap<RegisterUserCommand, ApplicationUser>().ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)).ReverseMap();

        }
    }
}