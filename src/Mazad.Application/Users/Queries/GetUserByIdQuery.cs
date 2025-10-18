using System;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDetailDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetUserByIdQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<UserDetailDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetUserByIdAsync(request.UserId, cancellationToken);
    }
}
