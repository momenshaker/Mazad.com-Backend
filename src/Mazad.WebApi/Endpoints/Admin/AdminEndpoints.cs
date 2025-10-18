using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Admin;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/dashboard", () =>
        {
            var metrics = new AdminDashboardResponse(1520, 324, 890000m, 48, 12);
            return Results.Ok(metrics);
        });

        group.MapGet("/listings", ([FromQuery] string? status, [FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var listings = new[]
            {
                new AdminListingSummary(Guid.NewGuid(), "2022 Mercedes G63", status ?? "PendingReview", DateTimeOffset.UtcNow.AddMinutes(-20))
            };

            return Results.Ok(new AdminListingListResponse(page, pageSize, listings));
        });

        group.MapPut("/listings/{listingId:guid}/approve", (Guid listingId, [FromBody] AdminReviewRequest request) =>
        {
            var result = new AdminModerationResponse(listingId, "Approved", request.Notes, DateTimeOffset.UtcNow);
            return Results.Ok(result);
        });

        group.MapPut("/listings/{listingId:guid}/reject", (Guid listingId, [FromBody] AdminReviewRequest request) =>
        {
            var result = new AdminModerationResponse(listingId, "Rejected", request.Notes, DateTimeOffset.UtcNow);
            return Results.Ok(result);
        });

        group.MapGet("/reports/transactions", ([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to) =>
        {
            var report = new TransactionReportResponse(from, to, 542, 1280000m);
            return Results.Ok(report);
        });

        group.MapGet("/reports/users", ([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to) =>
        {
            var report = new UserReportResponse(from, to, 820, 45);
            return Results.Ok(report);
        });

        group.MapGet("/audit-logs", ([FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var entries = new[]
            {
                new AuditLogEntry(DateTimeOffset.UtcNow.AddMinutes(-5), "admin@mazad.com", "Listings", "Approve", "Listing 123 approved")
            };

            return Results.Ok(new AuditLogResponse(page, pageSize, entries));
        });

        group.MapGet("/settings", () =>
        {
            var settings = new AdminSettingsResponse(0.05m, new[] { 100m, 250m, 500m }, "5-minute auto-extend up to 30 minutes");
            return Results.Ok(settings);
        });

        group.MapPut("/settings", ([FromBody] UpdateAdminSettingsRequest request) =>
        {
            var updated = new AdminSettingsResponse(request.PlatformFeePercentage, request.BidIncrements, request.AntiSnipingPolicy);
            return Results.Ok(updated);
        });

        return group;
    }

    public record AdminDashboardResponse(int ActiveAuctions, int PendingListings, decimal GmvThisMonth, int OpenDisputes, int PendingPayouts);

    public record AdminListingSummary(Guid ListingId, string Title, string Status, DateTimeOffset SubmittedAtUtc);

    public record AdminListingListResponse(int Page, int PageSize, IEnumerable<AdminListingSummary> Listings);

    public record AdminReviewRequest(string? Notes);

    public record AdminModerationResponse(Guid ListingId, string Status, string? Notes, DateTimeOffset ProcessedAtUtc);

    public record TransactionReportResponse(DateTimeOffset? From, DateTimeOffset? To, int TransactionsCount, decimal TotalVolume);

    public record UserReportResponse(DateTimeOffset? From, DateTimeOffset? To, int NewUsers, int SuspendedUsers);

    public record AuditLogEntry(DateTimeOffset OccurredAtUtc, string Actor, string Area, string Action, string Description);

    public record AuditLogResponse(int Page, int PageSize, IEnumerable<AuditLogEntry> Entries);

    public record AdminSettingsResponse(decimal PlatformFeePercentage, IEnumerable<decimal> BidIncrements, string AntiSnipingPolicy);

    public record UpdateAdminSettingsRequest(decimal PlatformFeePercentage, IEnumerable<decimal> BidIncrements, string AntiSnipingPolicy);
}
