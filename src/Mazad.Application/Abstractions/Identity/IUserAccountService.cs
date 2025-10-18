using Mazad.Application.Common.Models;

namespace Mazad.Application.Abstractions.Identity;

public interface IUserAccountService
{
    Task<AccountDto> GetAccountAsync(Guid userId, CancellationToken cancellationToken);

    Task<AccountDto> UpdateAccountAsync(
        Guid userId,
        string? fullName,
        string? phoneNumber,
        UpdateAccountProfileDto? profile,
        CancellationToken cancellationToken);
}
