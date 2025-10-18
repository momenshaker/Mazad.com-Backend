using Mazad.Application.Abstractions.Identity;
using MediatR;

namespace Mazad.Application.Users.Queries;

public record GetRolesQuery() : IRequest<IReadOnlyCollection<string>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyCollection<string>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetRolesQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<IReadOnlyCollection<string>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetRolesAsync(cancellationToken);
    }
}
