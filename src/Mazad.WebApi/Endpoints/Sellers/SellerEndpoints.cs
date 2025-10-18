using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Sellers;

/// <summary>
/// Provides extension methods for seller discovery and management endpoints.
/// </summary>
public static class SellerEndpoints
{
    /// <summary>
    /// Maps endpoints for exploring sellers and viewing seller-specific data.
    /// </summary>
    public static void MapSellerEndpoints(this IEndpointRouteBuilder routes)
    {
        var sellerGroup = routes.MapGroup("/api/v1/sellers");

        sellerGroup.MapGet("/", ([AsParameters] SellerDirectoryRequest request) =>
        {
            var sellers = new[]
            {
                new SellerSummary(Guid.NewGuid(), "Premium Motors", 4.8m, 12),
                new SellerSummary(Guid.NewGuid(), "Collector's Garage", 4.5m, 5)
            };

            return Results.Ok(new SellerDirectoryResponse(request.Page, request.PageSize, sellers));
        });

        sellerGroup.MapGet("/{sellerId:guid}", (Guid sellerId) =>
        {
            var seller = new SellerDetails(sellerId, "Premium Motors", "premium-motors", "Riyadh, Saudi Arabia", 4.8m, 245);
            return Results.Ok(seller);
        });

        sellerGroup.MapGet("/{sellerId:guid}/listings", (Guid sellerId, [FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var listings = new[]
            {
                new SellerListingSummary(Guid.NewGuid(), "2021 Toyota Land Cruiser", "Active", DateTimeOffset.UtcNow.AddDays(3), 120000m),
                new SellerListingSummary(Guid.NewGuid(), "2018 Ford F-150", "Draft", DateTimeOffset.UtcNow.AddDays(10), 90000m)
            };

            return Results.Ok(new SellerListingsResponse(sellerId, page, pageSize, listings));
        });

        sellerGroup.MapGet("/{sellerId:guid}/stats", (Guid sellerId) =>
        {
            var stats = new SellerStats(sellerId, 4.7m, 312, 284, 12, 6);
            return Results.Ok(stats);
        });

        var mySellingGroup = routes.MapGroup("/api/v1/me/selling")
            .RequireAuthorization("Scope:mazad.seller");

        mySellingGroup.MapGet("/", () =>
        {
            var lots = new MySellingResponse(
                new[]
                {
                    new SellerListingSummary(Guid.NewGuid(), "2019 Lexus LX 570", "Active", DateTimeOffset.UtcNow.AddDays(2), 210000m)
                },
                new[]
                {
                    new SellerListingSummary(Guid.NewGuid(), "2020 Nissan Patrol", "Draft", DateTimeOffset.UtcNow.AddDays(7), 0m)
                },
                new[]
                {
                    new SellerListingSummary(Guid.NewGuid(), "2015 BMW X5", "Completed", DateTimeOffset.UtcNow.AddDays(-3), 75000m)
                });

            return Results.Ok(lots);
        });

        mySellingGroup.MapGet("/payouts", () =>
        {
            var payouts = new[]
            {
                new PayoutSummary(Guid.NewGuid(), "Completed", 54000m, DateTimeOffset.UtcNow.AddDays(-2)),
                new PayoutSummary(Guid.NewGuid(), "Pending", 23000m, DateTimeOffset.UtcNow.AddDays(1))
            };

            return Results.Ok(new SellerPayoutsResponse(payouts));
        });
    }

    /// <summary>
    /// Request payload describing pagination when browsing sellers.
    /// </summary>
    public record SellerDirectoryRequest(int Page = 1, int PageSize = 20);

    /// <summary>
    /// Response payload containing paginated seller summaries.
    /// </summary>
    public record SellerDirectoryResponse(int Page, int PageSize, IEnumerable<SellerSummary> Sellers);

    /// <summary>
    /// Summary projection describing a seller in directory views.
    /// </summary>
    public record SellerSummary(Guid SellerId, string DisplayName, decimal Rating, int ActiveListings);

    /// <summary>
    /// Detailed response payload describing a seller profile.
    /// </summary>
    public record SellerDetails(Guid SellerId, string DisplayName, string Slug, string Location, decimal Rating, int Followers);

    /// <summary>
    /// Summary projection describing a listing owned by a seller.
    /// </summary>
    public record SellerListingSummary(Guid ListingId, string Title, string Status, DateTimeOffset EndAtUtc, decimal CurrentPrice);

    /// <summary>
    /// Response payload containing seller listings with pagination.
    /// </summary>
    public record SellerListingsResponse(Guid SellerId, int Page, int PageSize, IEnumerable<SellerListingSummary> Listings);

    /// <summary>
    /// Response payload summarizing seller performance metrics.
    /// </summary>
    public record SellerStats(Guid SellerId, decimal Rating, int SoldCount, int CompletedOrders, int ActiveListings, int DraftListings);

    /// <summary>
    /// Response payload describing the authenticated seller's listings by status.
    /// </summary>
    public record MySellingResponse(IEnumerable<SellerListingSummary> Active, IEnumerable<SellerListingSummary> Drafts, IEnumerable<SellerListingSummary> Completed);

    /// <summary>
    /// Summary projection describing a seller payout.
    /// </summary>
    public record PayoutSummary(Guid PayoutId, string Status, decimal Amount, DateTimeOffset ExpectedPayoutDateUtc);

    /// <summary>
    /// Response payload containing payout summaries for a seller.
    /// </summary>
    public record SellerPayoutsResponse(IEnumerable<PayoutSummary> Payouts);
}
