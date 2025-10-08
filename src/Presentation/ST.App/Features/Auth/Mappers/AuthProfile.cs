using AutoMapper;
using ST.App.Features.Auth.ViewModels;
using ST.Application.Features.Identity.Commands.LoginUser;
using ST.Application.Features.Identity.Commands.RegisterUser;

namespace ST.App.Features.Auth.Mappers
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterUserCommand, RegisterViewModel>().ReverseMap();
            CreateMap<LoginUserCommand, LoginViewModel>().ReverseMap();
        }
    }
}