using Mazad.Domain.Enums;

namespace Mazad.Application.Common.Models;

public record BidDto(
    Guid Id,
    decimal Amount,
    DateTimeOffset PlacedAt,
    BidStatus Status,
    Guid? BidderId,
    bool IsMine);
