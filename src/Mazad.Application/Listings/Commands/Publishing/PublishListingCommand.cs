using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Publishing;

public record PublishListingCommand(Guid ListingId, Guid ActorId, bool IsAdmin) : IRequest<ListingDto>;

public class PublishListingCommandHandler : IRequestHandler<PublishListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public PublishListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(PublishListingCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("You do not have permission to publish this listing.");
        }

        if (listing.Status == ListingStatus.Active)
        {
            return listing.ToDto();
        }

        if (listing.Status is ListingStatus.PendingReview or ListingStatus.Rejected)
        {
            throw new BusinessRuleException("Listing must be approved before it can be published.");
        }

        if (listing.EndAt is not null && listing.EndAt <= DateTimeOffset.UtcNow)
        {
            throw new BusinessRuleException("Listing end time has already passed.");
        }

        var now = DateTimeOffset.UtcNow;
        listing.Status = ListingStatus.Active;
        listing.StartAt ??= now;
        listing.EndAt ??= now.AddDays(7);
        listing.UpdatedAt = now;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
