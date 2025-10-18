using Mazad.Domain.Enums;

namespace Mazad.Application.Common.Models;

public record ListingDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategorySlug { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? Attributes { get; init; }
    public ListingType Type { get; init; }
    public ListingStatus Status { get; init; }
    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }
    public decimal? StartPrice { get; init; }
    public decimal? ReservePrice { get; init; }
    public decimal? BidIncrement { get; init; }
    public decimal? BuyNowPrice { get; init; }
    public int Views { get; init; }
    public int WatchCount { get; init; }
    public string? ModerationNotes { get; init; }
    public string? RejectionReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid? CreatedById { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public Guid? UpdatedById { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public Guid? DeletedById { get; init; }
    public IReadOnlyCollection<string> Flags { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<ListingMediaDto> Media { get; init; } = Array.Empty<ListingMediaDto>();
}

public record ListingMediaDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsCover { get; init; }
}
