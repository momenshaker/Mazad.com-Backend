using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record LoginUserCommand(string Email, string Password, bool RememberMe) : IRequest<LoginResultDto>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResultDto>
{
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<LoginResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(request.Email, request.Password, request.RememberMe, cancellationToken);
    }
}
