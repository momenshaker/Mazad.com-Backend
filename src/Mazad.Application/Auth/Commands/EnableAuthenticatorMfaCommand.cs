using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record EnableAuthenticatorMfaCommand(Guid UserId, string? Code) : IRequest<EnableAuthenticatorMfaResultDto>;

public class EnableAuthenticatorMfaCommandHandler : IRequestHandler<EnableAuthenticatorMfaCommand, EnableAuthenticatorMfaResultDto>
{
    private readonly IAuthService _authService;

    public EnableAuthenticatorMfaCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<EnableAuthenticatorMfaResultDto> Handle(EnableAuthenticatorMfaCommand request, CancellationToken cancellationToken)
    {
        return _authService.EnableAuthenticatorAsync(request.UserId, request.Code, cancellationToken);
    }
}
