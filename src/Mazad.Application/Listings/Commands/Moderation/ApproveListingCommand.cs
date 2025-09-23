using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Moderation;

public record ApproveListingCommand(Guid ListingId, Guid AdminId, string? Notes = null) : IRequest<ListingDto>;

public class ApproveListingCommandHandler : IRequestHandler<ApproveListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public ApproveListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(ApproveListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (listing.Status != ListingStatus.PendingReview)
        {
            throw new BusinessRuleException("Only listings pending review can be approved.");
        }

        listing.Status = listing.StartAt.HasValue && listing.StartAt > DateTimeOffset.UtcNow
            ? ListingStatus.Approved
            : ListingStatus.Active;
        listing.ModerationNotes = request.Notes;
        listing.RejectionReason = null;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.AdminId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
