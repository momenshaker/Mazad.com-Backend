using System;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Auctions;

public static class AuctionEndpoints
{
    public static RouteGroupBuilder MapAuctionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auctions")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/", () =>
        {
            var auctions = new[]
            {
                new AuctionSummary(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddDays(3), 1000m, 50m, true, "Standard")
            };

            return Results.Ok(new AuctionListResponse(auctions));
        });

        group.MapGet("/{listingId:guid}", (Guid listingId) =>
        {
            var auction = new AuctionDetails(
                listingId,
                DateTimeOffset.UtcNow.AddHours(-2),
                DateTimeOffset.UtcNow.AddDays(5),
                2000m,
                100m,
                true,
                "Dynamic 5-min anti-sniping");

            return Results.Ok(auction);
        });

        group.MapPost("/{listingId:guid}/cancel", (Guid listingId) =>
        {
            return Results.Ok(new AuctionActionResponse(listingId, "cancelled", DateTimeOffset.UtcNow));
        });

        group.MapPost("/{listingId:guid}/finalize", (Guid listingId) =>
        {
            return Results.Ok(new AuctionActionResponse(listingId, "finalized", DateTimeOffset.UtcNow));
        });

        return group;
    }

    public record AuctionSummary(Guid ListingId, Guid AuctionId, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, decimal ReservePrice, decimal BidIncrement, bool HasAntiSniping, string Policy);

    public record AuctionListResponse(IEnumerable<AuctionSummary> Auctions);

    public record AuctionDetails(Guid ListingId, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, decimal ReservePrice, decimal BidIncrement, bool AutoExtendEnabled, string AntiSnipingPolicy);

    public record AuctionActionResponse(Guid ListingId, string Action, DateTimeOffset ProcessedAtUtc);
}
