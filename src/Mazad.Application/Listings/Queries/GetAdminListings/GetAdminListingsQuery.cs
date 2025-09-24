using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Queries.GetAdminListings;

public record GetAdminListingsQuery(
    string? Search,
    Guid? SellerId,
    Guid? CategoryId,
    ListingStatus? Status,
    string? Sort,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ListingDto>>;

public class GetAdminListingsQueryHandler : IRequestHandler<GetAdminListingsQuery, PagedResult<ListingDto>>
{
    private readonly IMazadDbContext _context;

    public GetAdminListingsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingDto>> Handle(GetAdminListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Media)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(l => EF.Functions.Like(l.Title, $"%{term}%") || EF.Functions.Like(l.Description, $"%{term}%"));
        }

        if (request.SellerId.HasValue)
        {
            query = query.Where(l => l.SellerId == request.SellerId);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == request.CategoryId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status);
        }

        query = request.Sort switch
        {
            "created" => query.OrderBy(l => l.CreatedAt),
            "-created" => query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(l => l.ToDto()).ToList();
        return new PagedResult<ListingDto>(dtos, request.Page, request.PageSize, total);
    }
}
