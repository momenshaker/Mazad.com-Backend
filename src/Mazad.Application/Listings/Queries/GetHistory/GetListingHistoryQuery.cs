using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Bids;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Queries.GetHistory;

public record GetListingHistoryQuery(Guid ListingId) : IRequest<ListingHistoryDto>;

public class GetListingHistoryQueryHandler : IRequestHandler<GetListingHistoryQuery, ListingHistoryDto>
{
    private readonly IMazadDbContext _context;

    public GetListingHistoryQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingHistoryDto> Handle(GetListingHistoryQuery request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        var events = new List<ListingHistoryEventDto>
        {
            new("created", listing.CreatedAt, "Listing created", null, listing.CreatedById)
        };

        if (listing.UpdatedAt.HasValue)
        {
            events.Add(new ListingHistoryEventDto("updated", listing.UpdatedAt.Value, "Listing updated", null, listing.UpdatedById));
        }

        if (listing.Status == ListingStatus.Sold)
        {
            events.Add(new ListingHistoryEventDto("status", listing.UpdatedAt ?? listing.CreatedAt, "Listing marked as sold"));
        }

        var bids = await _context.Bids
            .AsNoTracking()
            .Where(b => b.ListingId == listing.Id)
            .OrderBy(b => b.PlacedAt)
            .ToListAsync(cancellationToken);

        events.AddRange(bids.Select(MapBidToEvent));

        return new ListingHistoryDto(listing.Id, events.OrderBy(e => e.OccurredAt).ToArray());
    }

    private static ListingHistoryEventDto MapBidToEvent(Bid bid)
    {
        return new ListingHistoryEventDto(
            "bid",
            bid.PlacedAt,
            $"Bid placed for {bid.Amount:C}",
            bid.Amount,
            bid.BidderId);
    }
}
