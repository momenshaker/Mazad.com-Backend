using System;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Users.Queries;

public record GetUserRatingsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserRatingDto>>;

public class GetUserRatingsQueryHandler : IRequestHandler<GetUserRatingsQuery, PagedResult<UserRatingDto>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetUserRatingsQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<PagedResult<UserRatingDto>> Handle(GetUserRatingsQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetUserRatingsAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
    }
}
