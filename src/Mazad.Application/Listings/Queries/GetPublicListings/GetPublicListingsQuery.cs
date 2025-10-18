using System;
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
    ListingStatus? Status,
    Guid? SellerId,
    decimal? PriceMin,
    decimal? PriceMax,
    bool EndingSoonOnly,
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
            .Include(l => l.Media);

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }
        else
        {
            query = query.Where(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Approved);
        }

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

        if (request.SellerId.HasValue)
        {
            query = query.Where(l => l.SellerId == request.SellerId.Value);
        }

        if (request.PriceMin.HasValue)
        {
            var min = request.PriceMin.Value;
            query = query.Where(l => (l.BuyNowPrice ?? l.StartPrice ?? 0) >= min);
        }

        if (request.PriceMax.HasValue)
        {
            var max = request.PriceMax.Value;
            query = query.Where(l => (l.BuyNowPrice ?? l.StartPrice ?? 0) <= max);
        }

        if (request.EndingSoonOnly)
        {
            var now = DateTimeOffset.UtcNow;
            var threshold = now.AddHours(24);
            query = query.Where(l => l.EndAt.HasValue && l.EndAt.Value >= now && l.EndAt.Value <= threshold);
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
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderByDescending(l => l.CreatedAt);
        }

        var term = sort
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(term))
        {
            return query.OrderByDescending(l => l.CreatedAt);
        }

        string field = term;
        string direction = "asc";

        if (term.Contains(':', StringComparison.Ordinal))
        {
            var parts = term.Split(':', 2, StringSplitOptions.TrimEntries);
            field = parts[0];
            direction = parts.Length > 1 ? parts[1] : direction;
        }
        else if (term.StartsWith('-'))
        {
            field = term[1..];
            direction = "desc";
        }
        else if (term.StartsWith('+'))
        {
            field = term[1..];
        }

        var descending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field.ToLowerInvariant() switch
        {
            "createdat" => descending
                ? query.OrderByDescending(l => l.CreatedAt)
                : query.OrderBy(l => l.CreatedAt),
            "endat" or "endingsoon" => descending
                ? query.OrderByDescending(l => l.EndAt)
                : query.OrderBy(l => l.EndAt),
            "price" => descending
                ? query.OrderByDescending(l => l.BuyNowPrice ?? l.StartPrice)
                : query.OrderBy(l => l.BuyNowPrice ?? l.StartPrice),
            "startprice" => descending
                ? query.OrderByDescending(l => l.StartPrice)
                : query.OrderBy(l => l.StartPrice),
            _ => descending
                ? query.OrderByDescending(l => l.CreatedAt)
                : query.OrderBy(l => l.CreatedAt)
        };
    }
}
