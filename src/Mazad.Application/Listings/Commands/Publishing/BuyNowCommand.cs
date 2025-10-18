using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Orders;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Publishing;

public record BuyNowCommand(Guid ListingId, Guid BuyerId) : IRequest<ListingDto>;

public class BuyNowCommandHandler : IRequestHandler<BuyNowCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public BuyNowCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (listing.Status != ListingStatus.Active)
        {
            throw new BusinessRuleException("Only active listings can be purchased immediately.");
        }

        if (listing.Type is not (ListingType.BuyNow or ListingType.Both))
        {
            throw new BusinessRuleException("This listing does not support Buy Now purchases.");
        }

        if (listing.BuyNowPrice is null)
        {
            throw new BusinessRuleException("No Buy Now price has been configured for this listing.");
        }

        var existingOrder = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ListingId == listing.Id && o.Status != OrderStatus.Cancelled, cancellationToken);

        if (existingOrder is not null)
        {
            throw new BusinessRuleException("A pending order already exists for this listing.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            BuyerId = request.BuyerId,
            SellerId = listing.SellerId,
            Price = listing.BuyNowPrice.Value,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = request.BuyerId
        };

        listing.Status = ListingStatus.Sold;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.BuyerId;

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
