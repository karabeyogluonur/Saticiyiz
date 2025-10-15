using Microsoft.AspNetCore.Http;
using ST.Application.Common.Constants;
using ST.Infrastructure.Extensions;

namespace ST.Infrastructure.Services.Common;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public int UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? throw new ApplicationException();

    public string EmailOrUsername => httpContextAccessor.HttpContext?.User.GetUsername() ?? throw new ApplicationException();

    public int TenantId => httpContextAccessor.HttpContext?.User.GetTenantId() ?? throw new ApplicationException();
}
