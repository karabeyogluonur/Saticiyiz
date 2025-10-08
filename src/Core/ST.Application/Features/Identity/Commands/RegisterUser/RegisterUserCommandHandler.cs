using MediatR;
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
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(IUserService userService, ITenantService tenantService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _tenantService = tenantService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            Response<ApplicationTenant> tenantResponse = await _tenantService.CreateTenantAsync();

            if (!tenantResponse.Succeeded)
                return new Response<int>(tenantResponse.Message, tenantResponse.Errors);

            ApplicationTenant applicationTenant = tenantResponse.Data;

            applicationTenant.AddDomainEvent(new TenantCreatedEvent(applicationTenant.Id, applicationTenant.Name));

            ApplicationUser user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                TenantId = applicationTenant.Id,
            };

            Response<int> userResponse = await _userService.CreateUserAsync(user, request.Password, applicationTenant.Id);

            if (!userResponse.Succeeded)
                return new Response<int>(userResponse.Message, userResponse.Errors);

            user.AddDomainEvent(new UserCreatedEvent(user.Id));

            await _unitOfWork.SaveChangesAsync();
            return new Response<int>(user.Id, "Kayıt işlemi başarıyla tamamlandı.");
        }
    }
}