using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Publishing;

public record ExtendListingCommand(Guid ListingId, Guid ActorId, bool IsAdmin, DateTimeOffset NewEndAt) : IRequest<ListingDto>;

public class ExtendListingCommandHandler : IRequestHandler<ExtendListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public ExtendListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(ExtendListingCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("You do not have permission to extend this listing.");
        }

        if (listing.Status is not (ListingStatus.Active or ListingStatus.Approved))
        {
            throw new BusinessRuleException("Only active or approved listings can be extended.");
        }

        if (listing.EndAt.HasValue && request.NewEndAt <= listing.EndAt.Value)
        {
            throw new BusinessRuleException("New end time must be later than the current end time.");
        }

        if (request.NewEndAt <= DateTimeOffset.UtcNow)
        {
            throw new BusinessRuleException("New end time must be in the future.");
        }

        listing.EndAt = request.NewEndAt;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
