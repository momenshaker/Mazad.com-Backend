using System;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;

namespace Mazad.Application.Abstractions.Identity;

public interface IIdentityAdminService
{
    Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, string? role, int page, int pageSize, CancellationToken cancellationToken);
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserDetailDto> UpdateUserAsync(Guid userId, UpdateUserAdminRequest request, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<PagedResult<UserRatingDto>> GetUserRatingsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<UserDetailDto> UpdateUserAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetRolesAsync(CancellationToken cancellationToken);
    Task CreateRoleAsync(string roleName, CancellationToken cancellationToken);
    Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken);
}

public record UpdateUserAdminRequest(
    string? FullName,
    string? PhoneNumber,
    bool? IsActive,
    bool? IsDeleted,
    KycStatus? KycStatus,
    bool? TwoFactorEnabled,
    IReadOnlyCollection<string>? Roles);
