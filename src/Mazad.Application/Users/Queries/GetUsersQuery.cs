using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Users.Queries;

public record GetUsersQuery(string? Search, string? Role, int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserSummaryDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetUsersQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<PagedResult<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetUsersAsync(request.Search, request.Role, request.Page, request.PageSize, cancellationToken);
    }
}
