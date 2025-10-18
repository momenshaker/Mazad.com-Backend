using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Publishing;

public record UnpublishListingCommand(Guid ListingId, Guid ActorId, bool IsAdmin) : IRequest<ListingDto>;

public class UnpublishListingCommandHandler : IRequestHandler<UnpublishListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public UnpublishListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(UnpublishListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (!request.IsAdmin && listing.SellerId != request.ActorId)
        {
            throw new BusinessRuleException("You do not have permission to unpublish this listing.");
        }

        if (listing.Status is not (ListingStatus.Active or ListingStatus.Approved))
        {
            throw new BusinessRuleException("Only active or approved listings can be unpublished.");
        }

        listing.Status = ListingStatus.Paused;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
