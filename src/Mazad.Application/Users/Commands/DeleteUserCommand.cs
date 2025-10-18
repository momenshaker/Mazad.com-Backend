using System;
using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Users.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IIdentityAdminService _identityAdminService;

    public DeleteUserCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _identityAdminService.DeleteUserAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}
