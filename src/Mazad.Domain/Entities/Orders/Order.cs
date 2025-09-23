using Mazad.Domain.Common;
using Mazad.Domain.Enums;

namespace Mazad.Domain.Entities.Orders;

public class Order : AuditableEntity
{
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
