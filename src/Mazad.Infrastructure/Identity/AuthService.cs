using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace Mazad.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string DefaultBidderRole = "Bidder";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<RegisterUserResultDto> RegisterAsync(string email, string password, string? fullName, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return new RegisterUserResultDto(false, existingUser.Id, existingUser.Email, new[] { "A user with this email already exists." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return new RegisterUserResultDto(false, null, email, createResult.Errors.Select(e => e.Description).ToArray());
        }

        await EnsureRoleExistsAsync(DefaultBidderRole);
        await _userManager.AddToRoleAsync(user, DefaultBidderRole);

        return new RegisterUserResultDto(true, user.Id, user.Email, Array.Empty<string>());
    }

    public async Task<LoginResultDto> LoginAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new LoginResultDto(false, false, false, new[] { "Invalid credentials." });
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return new LoginResultDto(true, false, false, Array.Empty<string>());
        }

        if (result.RequiresTwoFactor)
        {
            return new LoginResultDto(false, true, false, Array.Empty<string>());
        }

        if (result.IsLockedOut)
        {
            return new LoginResultDto(false, false, true, new[] { "User account is locked." });
        }

        return new LoginResultDto(false, false, false, new[] { "Invalid credentials." });
    }

    public async Task<ForgotPasswordResultDto> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new ForgotPasswordResultDto(true, null, Array.Empty<string>());
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return new ForgotPasswordResultDto(true, token, Array.Empty<string>());
    }

    public async Task<ResetPasswordResultDto> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new ResetPasswordResultDto(true, Array.Empty<string>());
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? new ResetPasswordResultDto(true, Array.Empty<string>())
            : new ResetPasswordResultDto(false, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<SetPasswordResultDto> SetPasswordAsync(Guid userId, string newPassword, string? currentPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            if (string.IsNullOrEmpty(currentPassword))
            {
                return new SetPasswordResultDto(false, true, new[] { "Current password is required to set a new password." });
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return changeResult.Succeeded
                ? new SetPasswordResultDto(true, false, Array.Empty<string>())
                : new SetPasswordResultDto(false, false, changeResult.Errors.Select(e => e.Description).ToArray());
        }

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        return addResult.Succeeded
            ? new SetPasswordResultDto(true, false, Array.Empty<string>())
            : new SetPasswordResultDto(false, false, addResult.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<EnableAuthenticatorMfaResultDto> EnableAuthenticatorAsync(Guid userId, string? code, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            var formattedKey = FormatKey(key!);
            var authenticatorUri = GenerateQrCodeUri(user.Email!, key!);
            return new EnableAuthenticatorMfaResultDto(
                RequiresVerification: true,
                SharedKey: formattedKey,
                AuthenticatorUri: authenticatorUri,
                RecoveryCodes: Array.Empty<string>(),
                Succeeded: false,
                Errors: Array.Empty<string>());
        }

        var sanitizedCode = code.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            sanitizedCode);

        if (!isValid)
        {
            return new EnableAuthenticatorMfaResultDto(
                RequiresVerification: false,
                SharedKey: FormatKey(key!),
                AuthenticatorUri: null,
                RecoveryCodes: Array.Empty<string>(),
                Succeeded: false,
                Errors: new[] { "Invalid verification code." });
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return new EnableAuthenticatorMfaResultDto(
            RequiresVerification: false,
            SharedKey: FormatKey(key!),
            AuthenticatorUri: null,
            RecoveryCodes: recoveryCodes.ToArray(),
            Succeeded: true,
            Errors: Array.Empty<string>());
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };

        await _roleManager.CreateAsync(role);
    }

    private static string FormatKey(string key)
    {
        var result = new StringBuilder();
        var currentPosition = 0;

        while (currentPosition + 4 < key.Length)
        {
            result.Append(key.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        result.Append(key.AsSpan(currentPosition));
        return result.ToString().ToLowerInvariant();
    }

    private static string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            UrlEncoder.Default.Encode("Mazad.com"),
            UrlEncoder.Default.Encode(email),
            UrlEncoder.Default.Encode(unformattedKey));
    }
}
