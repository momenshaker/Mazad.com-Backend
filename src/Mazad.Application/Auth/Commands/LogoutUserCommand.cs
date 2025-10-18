using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record LogoutUserCommand() : IRequest;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IAuthService _authService;

    public LogoutUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(cancellationToken);
        return Unit.Value;
    }
}
