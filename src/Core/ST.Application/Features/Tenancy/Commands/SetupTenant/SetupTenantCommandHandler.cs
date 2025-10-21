using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Common.Helpers;
using ST.Application.Exceptions;
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


namespace ST.Application.Features.Tenancy.Commands.SetupTenant
{
    public class SetupTenantCommandHandler : IRequestHandler<SetupTenantCommand, Response<int>>
    {
        private readonly IUserContext _userContext;
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        private readonly IPlanService _planService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SetupTenantCommandHandler(SignInManager<ApplicationUser> signInManager, ISubscriptionService subscriptionService, IUserContext userContext, IRoleService roleService, IUserService userService, ISettingService settingService, IUnitOfWork unitOfWork, ITenantService tenantService, IPlanService planService)
        {
            _userContext = userContext;
            _roleService = roleService;
            _userService = userService;
            _settingService = settingService;
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
            _planService = planService;
            _subscriptionService = subscriptionService;
            _signInManager = signInManager;
        }

        public async Task<Response<int>> Handle(SetupTenantCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            var tenantId = _userContext.TenantId;

            if (tenantId == null || userId == null)
                throw new UnauthorizedAccessException("Tenant or User context could not be found.");

            ApplicationTenant tenant = await _tenantService.GetTenantByIdAsync(tenantId);
            ApplicationUser user = await _userService.GetUserByIdAsync(userId);

            if (tenant == null || user == null)
                throw new NotFoundException("Tenant or User not found.");

            if (tenant.IsSetupCompleted)
                return Response<int>.Success(tenant.Id, "Kurulum işlemleriniz tamamlanmış gözüküyor.");

            int trialDays = await _settingService.GetValueAsync<int>(SettingKeyHelper.GetKey<SubscriptionSetting, int>(s => s.TrialPeriodDays));

            Plan defaultPlan = await _planService.GetTrialPlanAsync();

            if (defaultPlan == null || trialDays <= 0)
                return Response<int>.Error("System default settings are not configured.");

            await _roleService.SeedDefaultRolesForTenantAsync(tenantId);
            await _roleService.AssignRoleToUserAsync(user, AppRoles.Owner);
            await _subscriptionService.AssignTrialSubscriptionAsync(tenantId, defaultPlan.Id, trialDays);
            await _tenantService.MarkSetupAsCompletedAsync(tenant);
            await _userService.UpdateClaimAsync(user, CustomClaims.IsSetupCompleted, true.ToString());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _signInManager.RefreshSignInAsync(user);
            return Response<int>.Success(tenant.Id, "Kurulum işlemleriniz başarıyla tamamlanmıştır.");
        }
    }
}