using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Queries.GetSellerListings;

public record GetSellerListingsQuery(Guid SellerId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ListingDto>>;

public class GetSellerListingsQueryHandler : IRequestHandler<GetSellerListingsQuery, PagedResult<ListingDto>>
{
    private readonly IMazadDbContext _context;

    public GetSellerListingsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingDto>> Handle(GetSellerListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Media)
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(l => l.ToDto()).ToList();

        return new PagedResult<ListingDto>(dtos, request.Page, request.PageSize, total);
    }
}
