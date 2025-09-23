using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Queries.GetPublicListings;

public record GetPublicListingsQuery(
    string? Search,
    Guid? CategoryId,
    ListingType? Type,
    string? Sort,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ListingDto>>;

public class GetPublicListingsQueryHandler : IRequestHandler<GetPublicListingsQuery, PagedResult<ListingDto>>
{
    private readonly IMazadDbContext _context;

    public GetPublicListingsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingDto>> Handle(GetPublicListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Media)
            .Where(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Approved);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(l => EF.Functions.Like(l.Title, $"%{term}%") || EF.Functions.Like(l.Description, $"%{term}%"));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == request.CategoryId);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(l => l.Type == request.Type);
        }

        query = ApplySort(query, request.Sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(l => l.ToDto()).ToList();
        return new PagedResult<ListingDto>(dtos, request.Page, request.PageSize, total);
    }

    private static IQueryable<Domain.Entities.Listings.Listing> ApplySort(IQueryable<Domain.Entities.Listings.Listing> query, string? sort)
    {
        return sort switch
        {
            "price" => query.OrderBy(l => l.BuyNowPrice ?? l.StartPrice),
            "-price" => query.OrderByDescending(l => l.BuyNowPrice ?? l.StartPrice),
            "endAt" => query.OrderBy(l => l.EndAt),
            "-endAt" => query.OrderByDescending(l => l.EndAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };
    }
}
