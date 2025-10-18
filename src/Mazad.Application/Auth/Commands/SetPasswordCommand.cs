using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Auth.Commands;

public record SetPasswordCommand(Guid UserId, string NewPassword, string? CurrentPassword) : IRequest<SetPasswordResultDto>;

public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, SetPasswordResultDto>
{
    private readonly IAuthService _authService;

    public SetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<SetPasswordResultDto> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        return _authService.SetPasswordAsync(request.UserId, request.NewPassword, request.CurrentPassword, cancellationToken);
    }
}
