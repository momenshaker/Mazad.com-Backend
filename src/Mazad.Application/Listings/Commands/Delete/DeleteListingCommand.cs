using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Delete;

public record DeleteListingCommand(Guid ListingId, Guid ActorId, bool IsAdmin) : IRequest;

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public DeleteListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Bids)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (!request.IsAdmin && listing.SellerId != request.ActorId)
        {
            throw new BusinessRuleException("You do not have permission to delete this listing.");
        }

        if (listing.Bids.Any())
        {
            throw new BusinessRuleException("Listings with bids cannot be deleted.");
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
