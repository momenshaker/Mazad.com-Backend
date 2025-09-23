using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Watchlists.Commands;

public record RemoveFromWatchlistCommand(Guid UserId, Guid ListingId) : IRequest<Unit>;

public class RemoveFromWatchlistCommandHandler : IRequestHandler<RemoveFromWatchlistCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public RemoveFromWatchlistCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveFromWatchlistCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Watchlists
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ListingId == request.ListingId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException("WatchlistItem", request.ListingId);
        }

        _context.Watchlists.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
