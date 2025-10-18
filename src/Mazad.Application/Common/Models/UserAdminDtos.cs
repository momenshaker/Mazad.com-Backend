using System;
using Mazad.Domain.Enums;

namespace Mazad.Application.Common.Models;

public record UserSummaryDto(
    Guid Id,
    string? Email,
    string? FullName,
    bool EmailConfirmed,
    bool IsLockedOut,
    bool IsDeleted,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset? CreatedAt);

public record UserDetailDto(
    Guid Id,
    string? Email,
    string? FullName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLockedOut,
    bool IsDeleted,
    KycStatus KycStatus,
    bool TwoFactorEnabled,
    IReadOnlyCollection<string> Roles,
    UserProfileDto? Profile,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastSignInAt);

public record UserRatingDto(
    Guid Id,
    Guid FromUserId,
    Guid ToUserId,
    Guid? ListingId,
    int Score,
    string? Comment,
    DateTimeOffset CreatedAt);
