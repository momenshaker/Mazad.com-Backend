using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Accounts.Commands;

public record UpdateAccountCommand(
    Guid UserId,
    string? FullName,
    string? PhoneNumber,
    UpdateAccountProfileDto? Profile) : IRequest<AccountDto>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    private readonly IUserAccountService _userAccountService;

    public UpdateAccountCommandHandler(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    public Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        return _userAccountService.UpdateAccountAsync(
            request.UserId,
            request.FullName,
            request.PhoneNumber,
            request.Profile,
            cancellationToken);
    }
}
