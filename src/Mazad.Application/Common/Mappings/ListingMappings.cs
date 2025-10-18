using System;
using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Listings;
using Mazad.Domain.Enums;

namespace Mazad.Application.Common.Mappings;

public static class ListingMappings
{
    public static ListingDto ToDto(this Listing listing)
    {
        var flags = BuildFlags(listing);

        return new ListingDto
        {
            Id = listing.Id,
            SellerId = listing.SellerId,
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name,
            CategorySlug = listing.Category?.Slug,
            Title = listing.Title,
            Slug = listing.Slug,
            Description = listing.Description,
            Location = listing.Location,
            Attributes = listing.Attributes,
            Type = listing.Type,
            Status = listing.Status,
            StartAt = listing.StartAt,
            EndAt = listing.EndAt,
            StartPrice = listing.StartPrice,
            ReservePrice = listing.ReservePrice,
            BidIncrement = listing.BidIncrement,
            BuyNowPrice = listing.BuyNowPrice,
            Views = listing.Views,
            WatchCount = listing.WatchCount,
            ModerationNotes = listing.ModerationNotes,
            RejectionReason = listing.RejectionReason,
            CreatedAt = listing.CreatedAt,
            CreatedById = listing.CreatedById,
            UpdatedAt = listing.UpdatedAt,
            UpdatedById = listing.UpdatedById,
            IsDeleted = listing.IsDeleted,
            DeletedAt = listing.DeletedAt,
            DeletedById = listing.DeletedById,
            Flags = flags,
            Media = listing.Media
                .OrderBy(m => m.SortOrder)
                .Select(m => new ListingMediaDto
                {
                    Id = m.Id,
                    Url = m.Url,
                    Type = m.Type,
                    SortOrder = m.SortOrder,
                    IsCover = m.IsCover
                })
                .ToArray()
        };
    }

    private static IReadOnlyCollection<string> BuildFlags(Listing listing)
    {
        var flags = new List<string>();

        if (listing.IsDeleted)
        {
            flags.Add("archived");
        }

        if (!string.IsNullOrWhiteSpace(listing.ModerationNotes))
        {
            flags.Add("has-moderation-notes");
        }

        if (!string.IsNullOrWhiteSpace(listing.RejectionReason))
        {
            flags.Add("rejected");
        }

        if (listing.ReservePrice.HasValue)
        {
            flags.Add("reserve-set");
        }

        if (listing.BuyNowPrice.HasValue)
        {
            flags.Add("buy-now");
        }

        if (listing.EndAt is { } endAt)
        {
            var now = DateTimeOffset.UtcNow;
            if (endAt <= now.AddHours(24) && endAt >= now)
            {
                flags.Add("ending-soon");
            }
        }

        if (listing.Status == ListingStatus.PendingReview)
        {
            flags.Add("pending-review");
        }

        if (listing.Status == ListingStatus.Approved)
        {
            flags.Add("approved");
        }

        if (listing.Status == ListingStatus.Active)
        {
            flags.Add("active");
        }

        if (listing.Status == ListingStatus.Sold)
        {
            flags.Add("sold");
        }

        return flags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
