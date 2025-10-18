using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.UpdateStatus;

public record UpdateListingStatusCommand(Guid ListingId, Guid SellerId, ListingStatus TargetStatus, string? Reason = null) : IRequest<ListingDto>;

public class UpdateListingStatusCommandHandler : IRequestHandler<UpdateListingStatusCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public UpdateListingStatusCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(UpdateListingStatusCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (listing.SellerId != request.SellerId)
        {
            throw new BusinessRuleException("You do not have permission to change this listing status.");
        }

        EnsureTransitionAllowed(listing.Status, request.TargetStatus);

        listing.Status = request.TargetStatus;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.SellerId;

        if (request.TargetStatus == ListingStatus.Cancelled)
        {
            listing.ModerationNotes = request.Reason;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }

    private static void EnsureTransitionAllowed(ListingStatus current, ListingStatus target)
    {
        if (current == target)
        {
            return;
        }

        switch (target)
        {
            case ListingStatus.Paused when current != ListingStatus.Active:
                throw new BusinessRuleException("Only active listings can be paused.");
            case ListingStatus.Active when current != ListingStatus.Paused && current != ListingStatus.Approved:
                throw new BusinessRuleException("Only paused or approved listings can be activated by the seller.");
            case ListingStatus.Cancelled when current is ListingStatus.Draft or ListingStatus.PendingReview or ListingStatus.Active:
                break;
            default:
                if (target is ListingStatus.Sold or ListingStatus.Expired)
                {
                    throw new BusinessRuleException("The requested status change is managed by the platform.");
                }

                if (target is ListingStatus.PendingReview or ListingStatus.Approved or ListingStatus.Rejected)
                {
                    throw new BusinessRuleException("The requested status is managed by moderation.");
                }

                break;
        }
    }
}
