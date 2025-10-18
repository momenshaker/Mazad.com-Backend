using System.Linq;
using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Infrastructure.Identity;

public class UserAccountService : IUserAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMazadDbContext _context;

    public UserAccountService(UserManager<ApplicationUser> userManager, IMazadDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<AccountDto> GetAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<AccountDto> UpdateAccountAsync(
        Guid userId,
        string? fullName,
        string? phoneNumber,
        UpdateAccountProfileDto? profile,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;

        if (profile is not null)
        {
            if (user.Profile is null)
            {
                user.Profile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(user.Profile);
            }

            user.Profile.AvatarUrl = profile.AvatarUrl;
            user.Profile.Address = profile.Address;
            user.Profile.City = profile.City;
            user.Profile.Country = profile.Country;
            user.Profile.Language = profile.Language;
            user.Profile.Timezone = profile.Timezone;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    private static AccountDto MapToDto(ApplicationUser user, IEnumerable<string> roles)
    {
        var profileDto = user.Profile is null
            ? null
            : new UserProfileDto(
                user.Profile.AvatarUrl,
                user.Profile.Address,
                user.Profile.City,
                user.Profile.Country,
                user.Profile.Language,
                user.Profile.Timezone);

        return new AccountDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.KycStatus,
            user.TwoFactorEnabled,
            roles.ToArray(),
            profileDto);
    }
}
