using System;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Users.Commands;

public record UpdateUserAvatarCommand(Guid UserId, string AvatarUrl) : IRequest<UserDetailDto>;

public class UpdateUserAvatarCommandHandler : IRequestHandler<UpdateUserAvatarCommand, UserDetailDto>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateUserAvatarCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<UserDetailDto> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.UpdateUserAvatarAsync(request.UserId, request.AvatarUrl, cancellationToken);
    }
}
