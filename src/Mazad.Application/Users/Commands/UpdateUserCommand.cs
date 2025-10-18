using System;
using System.Collections.Generic;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Users.Commands;

public record UpdateUserCommand(Guid UserId, string? FullName, string? PhoneNumber, bool? IsActive, bool? IsDeleted, Mazad.Domain.Enums.KycStatus? KycStatus, bool? TwoFactorEnabled, IReadOnlyCollection<string>? Roles) : IRequest<UserDetailDto>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDetailDto>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateUserCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<UserDetailDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateUserAdminRequest(
            request.FullName,
            request.PhoneNumber,
            request.IsActive,
            request.IsDeleted,
            request.KycStatus,
            request.TwoFactorEnabled,
            request.Roles);

        return _identityAdminService.UpdateUserAsync(request.UserId, updateRequest, cancellationToken);
    }
}
