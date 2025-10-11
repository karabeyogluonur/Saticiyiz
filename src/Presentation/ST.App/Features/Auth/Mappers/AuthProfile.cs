using AutoMapper;
using ST.App.Features.Auth.ViewModels;
using ST.Application.Features.Identity.Commands.ForgotPassword;
using ST.Application.Features.Identity.Commands.LoginUser;
using ST.Application.Features.Identity.Commands.RegisterUser;
using ST.Application.Features.Identity.Commands.ResetPassword;

namespace ST.App.Features.Auth.Mappers
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterUserCommand, RegisterViewModel>().ReverseMap();
            CreateMap<LoginUserCommand, LoginViewModel>().ReverseMap();
            CreateMap<ForgotPasswordCommand, ForgotPasswordViewModel>().ReverseMap();
            CreateMap<ResetPasswordCommand, ResetPasswordViewModel>().ReverseMap();
        }
    }
}
