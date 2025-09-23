using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Domain.Entities.Watchlists;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Watchlists.Commands;

public record AddToWatchlistCommand(Guid UserId, Guid ListingId) : IRequest<Unit>;

public class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public AddToWatchlistCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddToWatchlistCommand request, CancellationToken cancellationToken)
    {
        var listingExists = await _context.Listings
            .AnyAsync(l => l.Id == request.ListingId, cancellationToken);

        if (!listingExists)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        var alreadyExists = await _context.Watchlists
            .AnyAsync(w => w.UserId == request.UserId && w.ListingId == request.ListingId, cancellationToken);

        if (alreadyExists)
        {
            return Unit.Value;
        }

        var now = DateTimeOffset.UtcNow;

        _context.Watchlists.Add(new WatchlistItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ListingId = request.ListingId,
            CreatedAt = now,
            CreatedById = request.UserId
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
