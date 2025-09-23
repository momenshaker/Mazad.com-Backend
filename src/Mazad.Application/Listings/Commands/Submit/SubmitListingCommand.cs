using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Submit;

public record SubmitListingCommand(Guid ListingId, Guid SellerId) : IRequest<ListingDto>;

public class SubmitListingCommandHandler : IRequestHandler<SubmitListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public SubmitListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(SubmitListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (listing.SellerId != request.SellerId)
        {
            throw new BusinessRuleException("You do not have permission to submit this listing.");
        }

        if (listing.Status != ListingStatus.Draft && listing.Status != ListingStatus.Rejected)
        {
            throw new BusinessRuleException("Only draft or rejected listings can be submitted for review.");
        }

        listing.Status = ListingStatus.PendingReview;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.SellerId;
        listing.RejectionReason = null;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
