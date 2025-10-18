using Mazad.Application.Common.Models;

namespace Mazad.Application.Abstractions.Identity;

public interface IAuthService
{
    Task<RegisterUserResultDto> RegisterAsync(string email, string password, string? fullName, CancellationToken cancellationToken);

    Task<LoginResultDto> LoginAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken);

    Task<ForgotPasswordResultDto> ForgotPasswordAsync(string email, CancellationToken cancellationToken);

    Task<ResetPasswordResultDto> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

    Task<SetPasswordResultDto> SetPasswordAsync(Guid userId, string newPassword, string? currentPassword, CancellationToken cancellationToken);

    Task<EnableAuthenticatorMfaResultDto> EnableAuthenticatorAsync(Guid userId, string? code, CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);
}
