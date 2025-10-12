using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Application.Common.Attributes;
using ST.Application.Common.Constants;
using ST.Application.Common.Helpers;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Subscriptions;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Settings;
using ST.Application.Wrappers;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Enums;
using ST.Domain.Identity;
using ST.Domain.Tenancy;

namespace ST.Application.Features.Identity.Commands.RegisterUser
{
    [Transactional]
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Response<int>>
    {
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;
        private readonly IRoleService _roleService;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantStore _currentTenantStore;

        public RegisterUserCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantStore currentTenantStore,
            IUserService userService,
            ITenantService tenantService,
            IRoleService roleService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantStore = currentTenantStore;

            _roleService = roleService;
            _tenantService = tenantService;
            _userService = userService;
        }

        public async Task<Response<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            bool emailExistsGlobally = await _userService.IsEmailUniqueGloballyAsync(request.Email);

            if (emailExistsGlobally)
                return Response<int>.Error($"'{request.Email}' e-posta adresi zaten başka bir hesap tarafından kullanılıyor.");

            bool phoneNumberExistsGlobally = await _userService.IsPhoneUniqueGloballyAsync(request.PhoneNumber);

            if (phoneNumberExistsGlobally)
                return Response<int>.Error($"'{request.PhoneNumber}' telefon numarası zaten başka bir hesap tarafından kullanılıyor.");

            ApplicationTenant applicationTenant = await _tenantService.CreateTenantAsync($"{request.FirstName} {request.LastName} Çalışma Alanı");

            applicationTenant.AddDomainEvent(new TenantCreatedEvent(applicationTenant.Id));

            _currentTenantStore.SetTenant(applicationTenant.Id);

            var (identityResult, applicationUser) = await _userService.CreateUserAsync(_mapper.Map<ApplicationUser>(request), applicationTenant, request.Password);

            if (!identityResult.Succeeded)
                return Response<int>.Error("Kullanıcı oluşturulurken bir hata oluştu.", identityResult.Errors.Select(e => e.Description));

            applicationUser.AddDomainEvent(new UserCreatedEvent(applicationUser.Id));

            await _roleService.AssignRoleToUserAsync(applicationUser, AppRoles.Owner);

            await _unitOfWork.SaveChangesAsync();

            return Response<int>.Success(applicationUser.Id, "Kayıt işlemi başarıyla tamamlandı.");
        }
    }
}
