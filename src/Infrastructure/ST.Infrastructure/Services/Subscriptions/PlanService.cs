using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Subscriptions;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Enums;
using System.Globalization;
using System.Threading.Tasks;

namespace ST.Infrastructure.Services.Subscriptions;

public class PlanService : IPlanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingService _settingService;

    public PlanService(IUnitOfWork unitOfWork, ISettingService settingService)
    {
        _unitOfWork = unitOfWork;
        _settingService = settingService;
    }

    public async Task<Plan> GetDefaultPlanAsync()
    {
        return await _unitOfWork.Plans.GetFirstOrDefaultAsync(predicate: plan => plan.IsDefault && plan.IsActive);
    }
    public async Task<Plan> GetTrialPlanAsync()
    {
        return await _unitOfWork.Plans.GetFirstOrDefaultAsync(predicate: plan => plan.IsTrial && plan.IsActive);
    }

}
