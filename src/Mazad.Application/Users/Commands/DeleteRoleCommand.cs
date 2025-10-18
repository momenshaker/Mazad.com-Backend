using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Users.Commands;

public record DeleteRoleCommand(string RoleName) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
{
    private readonly IIdentityAdminService _identityAdminService;

    public DeleteRoleCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        await _identityAdminService.DeleteRoleAsync(request.RoleName, cancellationToken);
        return Unit.Value;
    }
}
