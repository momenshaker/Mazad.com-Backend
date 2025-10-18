using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Media;

public record RemoveListingImageCommand(Guid ListingId, Guid ImageId, Guid ActorId, bool IsAdmin) : IRequest<Unit>;

public class RemoveListingImageCommandHandler : IRequestHandler<RemoveListingImageCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public RemoveListingImageCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveListingImageCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("You do not have permission to modify this listing.");
        }

        var media = listing.Media.FirstOrDefault(m => m.Id == request.ImageId);
        if (media is null)
        {
            throw new NotFoundException("ListingImage", request.ImageId);
        }

        listing.Media.Remove(media);
        _context.ListingMedia.Remove(media);

        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
