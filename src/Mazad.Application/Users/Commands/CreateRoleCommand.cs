using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Users.Commands;

public record CreateRoleCommand(string RoleName) : IRequest;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Unit>
{
    private readonly IIdentityAdminService _identityAdminService;

    public CreateRoleCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task<Unit> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        await _identityAdminService.CreateRoleAsync(request.RoleName, cancellationToken);
        return Unit.Value;
    }
}
