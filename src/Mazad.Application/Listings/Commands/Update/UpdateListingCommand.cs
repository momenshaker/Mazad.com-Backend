using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Update;

public record UpdateListingCommand : IRequest<ListingDto>
{
    public Guid ListingId { get; init; }
    public Guid ActorId { get; init; }
    public bool IsAdmin { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public string? Attributes { get; init; }
    public ListingType? Type { get; init; }
    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }
    public decimal? StartPrice { get; init; }
    public decimal? ReservePrice { get; init; }
    public decimal? BidIncrement { get; init; }
    public decimal? BuyNowPrice { get; init; }
}

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public UpdateListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (!request.IsAdmin && listing.SellerId != request.ActorId)
        {
            throw new BusinessRuleException("You do not have permission to modify this listing.");
        }

        var hasBids = await _context.Bids
            .AsNoTracking()
            .AnyAsync(b => b.ListingId == request.ListingId, cancellationToken);

        if (request.CategoryId.HasValue && !hasBids)
        {
            listing.CategoryId = request.CategoryId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Title) && !hasBids)
        {
            listing.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            listing.Description = request.Description;
        }

        if (request.Location is not null)
        {
            listing.Location = request.Location;
        }

        if (request.Attributes is not null)
        {
            listing.Attributes = request.Attributes;
        }

        if (request.Type.HasValue && !hasBids)
        {
            listing.Type = request.Type.Value;
        }

        if (request.StartAt.HasValue && !hasBids)
        {
            listing.StartAt = request.StartAt;
        }

        if (request.EndAt.HasValue && !hasBids)
        {
            listing.EndAt = request.EndAt;
        }

        if (request.StartPrice.HasValue && !hasBids)
        {
            listing.StartPrice = request.StartPrice;
        }

        if (request.ReservePrice.HasValue && !hasBids)
        {
            listing.ReservePrice = request.ReservePrice;
        }

        if (request.BidIncrement.HasValue && !hasBids)
        {
            listing.BidIncrement = request.BidIncrement;
        }

        if (request.BuyNowPrice.HasValue && !hasBids)
        {
            listing.BuyNowPrice = request.BuyNowPrice;
        }

        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
