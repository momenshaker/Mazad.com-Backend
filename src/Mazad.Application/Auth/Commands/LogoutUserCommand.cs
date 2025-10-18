using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record LogoutUserCommand() : IRequest<Unit>;

public class LogoutUserCommandHandler(IAuthService authService) : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IAuthService _authService = authService;

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(cancellationToken);
        return Unit.Value;
    }
}
