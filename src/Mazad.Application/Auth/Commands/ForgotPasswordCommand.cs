using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResultDto>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResultDto>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<ForgotPasswordResultDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return _authService.ForgotPasswordAsync(request.Email, cancellationToken);
    }
}
