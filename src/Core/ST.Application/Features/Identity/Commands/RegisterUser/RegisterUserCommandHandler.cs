using AutoMapper;
using MediatR;
using ST.Application.Common.Attributes;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.RegisterUser
{
    [Transactional]
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Response<int>>
    {
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(
            IUserService userService,
            ITenantService tenantService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userService = userService;
            _tenantService = tenantService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (!await _userService.IsEmailUniqueGloballyAsync(request.Email))
                return Response<int>.Error($"'{request.Email}' e-posta adresi zaten kullanılıyor.");

            if (!await _userService.IsPhoneUniqueGloballyAsync(request.PhoneNumber))
                return Response<int>.Error($"'{request.PhoneNumber}' telefon numarası zaten kullanılıyor.");

            var newTenant = await _tenantService.CreateTenantAsync($"{request.FirstName} {request.LastName} Çalışma Alanı");

            var user = _mapper.Map<ApplicationUser>(request);

            var (identityResult, createdUser) = await _userService.CreateUserAsync(user, newTenant, request.Password);

            if (!identityResult.Succeeded)
                return Response<int>.Error("Kullanıcı oluşturulurken bir hata oluştu.", identityResult.Errors.Select(e => e.Description));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Response<int>.Success(createdUser.Id, "Hesabınız başarıyla oluşturuldu. Kurulumu tamamlamak için lütfen giriş yapın.");
        }
    }
}