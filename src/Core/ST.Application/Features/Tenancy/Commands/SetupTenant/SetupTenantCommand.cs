using MediatR;
using ST.Application.Common.Attributes;
using ST.Application.Wrappers;

namespace ST.Application.Features.Tenancy.Commands.SetupTenant
{
    [Transactional]
    public class SetupTenantCommand : IRequest<Response<int>>
    {
    }
}