using Mazad.Domain.Entities.Admin;
using Mazad.Domain.Entities.Bids;
using Mazad.Domain.Entities.Catalog;
using Mazad.Domain.Entities.Cms;
using Mazad.Domain.Entities.Identity;
using Mazad.Domain.Entities.Listings;
using Mazad.Domain.Entities.Orders;
using Mazad.Domain.Entities.Taxonomy;
using Mazad.Domain.Entities.Watchlists;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Abstractions.Persistence;

public interface IMazadDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<AttributeDefinition> AttributeDefinitions { get; }
    DbSet<VehicleBrand> VehicleBrands { get; }
    DbSet<VehicleModel> VehicleModels { get; }
    DbSet<VehicleTrim> VehicleTrims { get; }
    DbSet<YearRange> YearRanges { get; }
    DbSet<Listing> Listings { get; }
    DbSet<ListingMedia> ListingMedia { get; }
    DbSet<Bid> Bids { get; }
    DbSet<WatchlistItem> Watchlists { get; }
    DbSet<Order> Orders { get; }
    DbSet<Page> Pages { get; }
    DbSet<Menu> Menus { get; }
    DbSet<MediaAsset> MediaAssets { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<AuditLogEntry> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
