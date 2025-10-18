using Mazad.Application.Abstractions.Persistence;
using Mazad.Domain.Entities.Admin;
using Mazad.Domain.Entities.Bids;
using Mazad.Domain.Entities.Catalog;
using Mazad.Domain.Entities.Cms;
using Mazad.Domain.Entities.Identity;
using Mazad.Domain.Entities.Listings;
using Mazad.Domain.Entities.Orders;
using Mazad.Domain.Entities.Taxonomy;
using Mazad.Domain.Entities.Watchlists;
using Mazad.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Infrastructure.Persistence;

public class MazadDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IMazadDbContext
{
    public MazadDbContext(DbContextOptions<MazadDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<VehicleBrand> VehicleBrands => Set<VehicleBrand>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<VehicleTrim> VehicleTrims => Set<VehicleTrim>();
    public DbSet<YearRange> YearRanges => Set<YearRange>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingMedia> ListingMedia => Set<ListingMedia>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<WatchlistItem> Watchlists => Set<WatchlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Listing>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.StartPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReservePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BidIncrement).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BuyNowPrice).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Media)
                .WithOne(m => m.Listing)
                .HasForeignKey(m => m.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Bids)
                .WithOne(b => b.Listing)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ListingMedia>(entity =>
        {
            entity.Property(e => e.Url).HasMaxLength(2048);
        });

        builder.Entity<Bid>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        });

        builder.Entity<WatchlistItem>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ListingId }).IsUnique();
        });

        builder.Entity<Page>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        builder.Entity<Menu>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(e => e.Profile)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserProfile>(entity =>
        {
            entity.Property(e => e.AvatarUrl).HasMaxLength(2048);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Domain.Common.AuditableEntity auditable)
            {
                var now = DateTimeOffset.UtcNow;
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = now;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
