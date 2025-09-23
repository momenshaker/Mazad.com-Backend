using System.Linq;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Listings;

namespace Mazad.Application.Common.Mappings;

public static class ListingMappings
{
    public static ListingDto ToDto(this Listing listing)
    {
        return new ListingDto
        {
            Id = listing.Id,
            SellerId = listing.SellerId,
            CategoryId = listing.CategoryId,
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
}
