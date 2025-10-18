using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Admin;

/// <summary>
/// Provides extension methods for administrative dashboard and configuration endpoints.
/// </summary>
public static class AdminEndpoints
{
    /// <summary>
    /// Maps administrative endpoints for dashboards, reports, and settings.
    /// </summary>
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/dashboard", () =>
        {
            var metrics = new AdminDashboardResponse(1520, 324, 890000m, 48, 12);
            return Results.Ok(metrics);
        });

        group.MapGet("/dashboard/listings", ([FromQuery] string? status, [FromQuery] int page, [FromQuery] int pageSize) =>
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

    /// <summary>
    /// Response payload summarizing headline dashboard metrics.
    /// </summary>
    public record AdminDashboardResponse(int ActiveAuctions, int PendingListings, decimal GmvThisMonth, int OpenDisputes, int PendingPayouts);

    /// <summary>
    /// Summary projection describing a listing awaiting moderation.
    /// </summary>
    public record AdminListingSummary(Guid ListingId, string Title, string Status, DateTimeOffset SubmittedAtUtc);

    /// <summary>
    /// Response payload containing a paginated collection of admin listing summaries.
    /// </summary>
    public record AdminListingListResponse(int Page, int PageSize, IEnumerable<AdminListingSummary> Listings);

    /// <summary>
    /// Request payload submitted when reviewing a listing.
    /// </summary>
    public record AdminReviewRequest(string? Notes);

    /// <summary>
    /// Response payload describing the outcome of an admin moderation action.
    /// </summary>
    public record AdminModerationResponse(Guid ListingId, string Status, string? Notes, DateTimeOffset ProcessedAtUtc);

    /// <summary>
    /// Response payload summarizing transaction report statistics.
    /// </summary>
    public record TransactionReportResponse(DateTimeOffset? From, DateTimeOffset? To, int TransactionsCount, decimal TotalVolume);

    /// <summary>
    /// Response payload summarizing user account trends.
    /// </summary>
    public record UserReportResponse(DateTimeOffset? From, DateTimeOffset? To, int NewUsers, int SuspendedUsers);

    /// <summary>
    /// Representation of a single administrative audit log entry.
    /// </summary>
    public record AuditLogEntry(DateTimeOffset OccurredAtUtc, string Actor, string Area, string Action, string Description);

    /// <summary>
    /// Response payload containing paginated audit log entries.
    /// </summary>
    public record AuditLogResponse(int Page, int PageSize, IEnumerable<AuditLogEntry> Entries);

    /// <summary>
    /// Response payload describing configurable platform settings.
    /// </summary>
    public record AdminSettingsResponse(decimal PlatformFeePercentage, IEnumerable<decimal> BidIncrements, string AntiSnipingPolicy);

    /// <summary>
    /// Request payload used to update administrative platform settings.
    /// </summary>
    public record UpdateAdminSettingsRequest(decimal PlatformFeePercentage, IEnumerable<decimal> BidIncrements, string AntiSnipingPolicy);
}
