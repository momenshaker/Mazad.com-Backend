using System;
using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using Mazad.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Infrastructure.Identity;

public class IdentityAdminService : IIdentityAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMazadDbContext _context;

    public IdentityAdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMazadDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, string? role, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.Email ?? string.Empty, $"%{term}%") ||
                EF.Functions.Like(u.FullName ?? string.Empty, $"%{term}%") ||
                EF.Functions.Like(u.PhoneNumber ?? string.Empty, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleName = role.Trim();
            var roleUsers = await _userManager.GetUsersInRoleAsync(roleName);
            var roleUserIds = roleUsers.Select(u => u.Id).ToArray();
            query = query.Where(u => roleUserIds.Contains(u.Id));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserSummaryDto>(users.Count);
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();
            items.Add(new UserSummaryDto(
                user.Id,
                user.Email,
                user.FullName,
                user.EmailConfirmed,
                await _userManager.IsLockedOutAsync(user),
                user.IsDeleted,
                roles,
                null));
        }

        return new PagedResult<UserSummaryDto>(items, page, pageSize, total);
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var profile = user.Profile is null
            ? null
            : new UserProfileDto(
                user.Profile.AvatarUrl,
                user.Profile.Address,
                user.Profile.City,
                user.Profile.Country,
                user.Profile.Language,
                user.Profile.Timezone);

        return new UserDetailDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.EmailConfirmed,
            await _userManager.IsLockedOutAsync(user),
            user.IsDeleted,
            user.KycStatus,
            user.TwoFactorEnabled,
            roles,
            profile,
            null,
            null);
    }

    public async Task<UserDetailDto> UpdateUserAsync(Guid userId, UpdateUserAdminRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (request.FullName is not null)
        {
            user.FullName = request.FullName;
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.KycStatus.HasValue)
        {
            user.KycStatus = request.KycStatus.Value;
        }

        if (request.IsDeleted.HasValue)
        {
            user.IsDeleted = request.IsDeleted.Value;
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
        }

        if (request.TwoFactorEnabled.HasValue)
        {
            user.TwoFactorEnabled = request.TwoFactorEnabled.Value;
        }

        if (request.Roles is not null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var targetRoles = request.Roles.Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)).ToArray();

            var rolesToRemove = currentRoles.Except(targetRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            var rolesToAdd = targetRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    throw new BusinessRuleException("Failed to remove user roles.");
                }
            }

            if (rolesToAdd.Length > 0)
            {
                foreach (var roleName in rolesToAdd)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new ApplicationRole(roleName));
                    }
                }

                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    throw new BusinessRuleException("Failed to add user roles.");
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(userId, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        user.IsDeleted = true;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<PagedResult<UserRatingDto>> GetUserRatingsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var empty = Array.Empty<UserRatingDto>();
        return Task.FromResult(new PagedResult<UserRatingDto>(empty, page, pageSize, 0));
    }

    public async Task<UserDetailDto> UpdateUserAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (user.Profile is null)
        {
            user.Profile = new UserProfile { UserId = user.Id };
            await _context.UserProfiles.AddAsync(user.Profile, cancellationToken);
        }

        user.Profile.AvatarUrl = avatarUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name ?? string.Empty)
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task CreateRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            throw new ConflictException("Role", roleName);
        }

        var result = await _roleManager.CreateAsync(new ApplicationRole(roleName));
        if (!result.Succeeded)
        {
            throw new BusinessRuleException("Failed to create role.");
        }
    }

    public async Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException("Role", roleName);
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException("Failed to delete role.");
        }
    }
}
