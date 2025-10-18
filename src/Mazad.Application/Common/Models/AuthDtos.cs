namespace Mazad.Application.Common.Models;

public record RegisterUserResultDto(bool Succeeded, Guid? UserId, string? Email, string[] Errors);

public record LoginResultDto(bool Succeeded, bool RequiresTwoFactor, bool IsLockedOut, string[] Errors);

public record ForgotPasswordResultDto(bool Succeeded, string? ResetToken, string[] Errors);

public record ResetPasswordResultDto(bool Succeeded, string[] Errors);

public record SetPasswordResultDto(bool Succeeded, bool RequiresCurrentPassword, string[] Errors);

public record EnableAuthenticatorMfaResultDto(
    bool RequiresVerification,
    string? SharedKey,
    string? AuthenticatorUri,
    IReadOnlyCollection<string> RecoveryCodes,
    bool Succeeded,
    string[] Errors);
