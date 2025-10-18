using Mazad.Application.Abstractions.Identity;
using Mazad.Application.Common.Models;
using MediatR;

namespace Mazad.Application.Accounts.Queries;

public record GetCurrentAccountQuery(Guid UserId) : IRequest<AccountDto>;

public class GetCurrentAccountQueryHandler : IRequestHandler<GetCurrentAccountQuery, AccountDto>
{
    private readonly IUserAccountService _userAccountService;

    public GetCurrentAccountQueryHandler(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    public Task<AccountDto> Handle(GetCurrentAccountQuery request, CancellationToken cancellationToken)
    {
        return _userAccountService.GetAccountAsync(request.UserId, cancellationToken);
    }
}
