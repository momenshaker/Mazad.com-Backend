using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Moderation;

public record RejectListingCommand(Guid ListingId, Guid AdminId, string Reason) : IRequest<ListingDto>;

public class RejectListingCommandHandler : IRequestHandler<RejectListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public RejectListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(RejectListingCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("Only listings pending review can be rejected.");
        }

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = request.Reason;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.AdminId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
