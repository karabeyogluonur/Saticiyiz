using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Application.Common.Attributes;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Wrappers;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using ST.Domain.Events.Tenancy;
using ST.Domain.Identity;

namespace ST.Application.Features.Identity.Commands.RegisterUser
{
    [Transactional]
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Response<int>>
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserService userService, ITenantService tenantService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userService = userService;
            _tenantService = tenantService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            Response<ApplicationTenant> tenantResponse = await _tenantService.CreateTenantAsync();

            ApplicationTenant applicationTenant = tenantResponse.Data;

            applicationTenant.AddDomainEvent(new TenantCreatedEvent(applicationTenant.Id, applicationTenant.Name));

            ApplicationUser applicationUser = _mapper.Map<ApplicationUser>(request);
            applicationUser.TenantId = applicationTenant.Id;


            bool userWithSameEmail = await _userManager.Users.AnyAsync(user => user.Email == request.Email);

            if (userWithSameEmail)
                return Response<int>.Error($"'{request.Email}' e-posta adresi zaten kullanılıyor.");

            bool userWithSamePhone = await _userManager.Users.AnyAsync(user => user.PhoneNumber == request.PhoneNumber);

            if (userWithSamePhone)
                return Response<int>.Error($"'{request.PhoneNumber}' telefon numarası zaten kullanılıyor.");

            IdentityResult result = await _userManager.CreateAsync(applicationUser, request.Password);

            if (!result.Succeeded)
                return Response<int>.Error("Kullanıcı oluşturulurken bir hata oluştu.", result.Errors.Select(e => e.Description));

            applicationUser.AddDomainEvent(new UserCreatedEvent(applicationUser.Id));

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.Success(applicationUser.Id, "Kayıt işlemi başarıyla tamamlandı.");
        }
    }
}