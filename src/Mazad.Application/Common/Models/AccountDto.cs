using Mazad.Domain.Enums;

namespace Mazad.Application.Common.Models;

public record AccountDto(
    Guid Id,
    string? Email,
    string? FullName,
    string? PhoneNumber,
    KycStatus KycStatus,
    bool TwoFactorEnabled,
    IReadOnlyCollection<string> Roles,
    UserProfileDto? Profile);

public record UserProfileDto(
    string? AvatarUrl,
    string? Address,
    string? City,
    string? Country,
    string? Language,
    string? Timezone);

public record UpdateAccountProfileDto(
    string? AvatarUrl,
    string? Address,
    string? City,
    string? Country,
    string? Language,
    string? Timezone);
