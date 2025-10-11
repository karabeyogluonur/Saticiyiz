using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Application.Common.Attributes;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantStore _currentTenantStore;
        private readonly ISettingService _settingService;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RegisterUserCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ICurrentTenantStore currentTenantStore,
            ISettingService settingService,
            RoleManager<ApplicationRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _currentTenantStore = currentTenantStore;
            _settingService = settingService;
            _roleManager = roleManager;
        }

        public async Task<Response<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var emailExistsGlobally = await _unitOfWork.Users.GetAll(tracking: false)
                .IgnoreQueryFilters()
                .AnyAsync(u => u.NormalizedEmail == _userManager.NormalizeEmail(request.Email), cancellationToken);

            if (emailExistsGlobally)
                return Response<int>.Error($"'{request.Email}' e-posta adresi zaten başka bir hesap tarafından kullanılıyor.");

            var phoneNumberExistsGlobally = await _unitOfWork.Users.GetAll(tracking: false)
                .IgnoreQueryFilters()
                .AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (phoneNumberExistsGlobally)
                return Response<int>.Error($"'{request.PhoneNumber}' telefon numarası zaten başka bir hesap tarafından kullanılıyor.");

            var trialDays = await _settingService.GetValueAsync<int>("Subscription.TrialPeriodDays");

            if (trialDays <= 0)
                return Response<int>.Error("Sistemde deneme süresi tanımlanmamış. Lütfen yönetici ile iletişime geçin.");

            var defaultPlan = await _unitOfWork.Plans.GetAsync(p => p.IsDefault);

            if (defaultPlan == null)
                return Response<int>.Error("Varsayılan abonelik planı bulunamadı. Lütfen yönetici ile iletişime geçin.");

            ApplicationTenant newTenant = new ApplicationTenant
            {
                Name = $"{request.FirstName} {request.LastName} Workspaces",
                Identifier = Guid.NewGuid().ToString(),
                CreatedBy = "system",
            };

            var newSubscription = new Subscription
            {
                Tenant = newTenant,
                PlanId = defaultPlan.Id,
                Status = SubscriptionStatus.Trial,
                CurrentPeriodEndDate = DateTime.UtcNow.AddDays(trialDays),
                CreatedBy = "System.Registration",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Tenants.AddAsync(newTenant);
            await _unitOfWork.Subscriptions.AddAsync(newSubscription);
            newTenant.AddDomainEvent(new TenantCreatedEvent(newTenant.Id));
            await _unitOfWork.Tenants.AddAsync(newTenant);

            _currentTenantStore.SetTenant(newTenant.Id);

            var applicationUser = _mapper.Map<ApplicationUser>(request);

            applicationUser.Tenant = newTenant;
            applicationUser.UserName = request.Email;

            var result = await _userManager.CreateAsync(applicationUser, request.Password);

            if (!result.Succeeded)
                return Response<int>.Error("Kullanıcı oluşturulurken bir hata oluştu.", result.Errors.Select(e => e.Description));

            await CreateDefaultTenantRoles(newTenant);

            var roleResult = await _userManager.AddToRoleAsync(applicationUser, AppRoles.Owner);

            if (!roleResult.Succeeded)
                return Response<int>.Error("Kullanıcı oluşturuldu ancak rol atanamadı.", roleResult.Errors.Select(e => e.Description));

            applicationUser.AddDomainEvent(new UserCreatedEvent(applicationUser.Id));

            return Response<int>.Success(applicationUser.Id, "Kayıt işlemi başarıyla tamamlandı.");
        }
        public async Task CreateDefaultTenantRoles(ApplicationTenant tenant)
        {
            List<string> roles = typeof(AppRoles)
            .GetFields(System.Reflection.BindingFlags.Public |
                       System.Reflection.BindingFlags.Static |
                       System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetRawConstantValue()?.ToString())
            .ToList();

            foreach (var roleName in roles)
            {
                var roleExists = await _roleManager.Roles
                    .AnyAsync(r => r.TenantId == tenant.Id && r.Name == roleName);

                if (!roleExists)
                {
                    var newRole = new ApplicationRole()
                    {
                        Description = $"Organizasyon için varsayılan {roleName} rolü.",
                        Name = roleName,
                        Tenant = tenant
                    };
                    await _roleManager.CreateAsync(newRole);
                }
            }
        }
    }
}
