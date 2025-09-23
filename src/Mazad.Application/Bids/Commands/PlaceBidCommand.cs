using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Domain.Entities.Bids;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Bids.Commands;

public record PlaceBidCommand(Guid ListingId, Guid BidderId, decimal Amount) : IRequest<Guid>;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Guid>
{
    private readonly IMazadDbContext _context;

    public PlaceBidCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Bids)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (listing.Type == ListingType.BuyNow && !listing.StartPrice.HasValue)
        {
            throw new BusinessRuleException("Bidding is not enabled for this listing.");
        }

        if (listing.Status != ListingStatus.Active)
        {
            throw new BusinessRuleException("Only active listings accept bids.");
        }

        if (listing.EndAt.HasValue && listing.EndAt <= DateTimeOffset.UtcNow)
        {
            throw new BusinessRuleException("The auction has already ended.");
        }

        var highestBid = listing.Bids
            .Where(b => b.Status == BidStatus.Placed || b.Status == BidStatus.Winning)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

        var minimumBid = highestBid != null
            ? highestBid.Amount + (listing.BidIncrement ?? 1)
            : listing.StartPrice ?? 1;

        if (request.Amount < minimumBid)
        {
            throw new BusinessRuleException($"Bid amount must be at least {minimumBid:0.##}.");
        }

        if (highestBid != null)
        {
            highestBid.Status = BidStatus.Outbid;
            highestBid.UpdatedAt = DateTimeOffset.UtcNow;
            highestBid.UpdatedById = request.BidderId;
        }

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            BidderId = request.BidderId,
            Amount = request.Amount,
            PlacedAt = DateTimeOffset.UtcNow,
            Status = BidStatus.Winning,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = request.BidderId
        };

        listing.Bids.Add(bid);

        await _context.SaveChangesAsync(cancellationToken);
        return bid.Id;
    }
}
