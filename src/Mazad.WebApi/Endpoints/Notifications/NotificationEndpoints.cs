using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Notifications;

/// <summary>
/// Provides extension methods for user notification endpoints.
/// </summary>
public static class NotificationEndpoints
{
    /// <summary>
    /// Maps endpoints for retrieving notifications and updating preferences.
    /// </summary>
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications")
            .RequireAuthorization("Scope:mazad.api");

        group.MapGet("/", ([FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var notifications = new[]
            {
                new NotificationSummary(Guid.NewGuid(), "listing", "Auction ending soon", "listing-123", false, DateTimeOffset.UtcNow.AddMinutes(-15))
            };

            return Results.Ok(new NotificationListResponse(page, pageSize, notifications));
        });

        group.MapPost("/mark-read", ([FromBody] MarkNotificationsReadRequest request) =>
        {
            return Results.Ok(new MarkNotificationsReadResponse(request.NotificationIds, DateTimeOffset.UtcNow));
        });

        group.MapPost("/settings", ([FromBody] UpdateNotificationSettingsRequest request) =>
        {
            var settings = new NotificationSettingsResponse(request.Email, request.Sms, request.Push, DateTimeOffset.UtcNow);
            return Results.Ok(settings);
        });

        return group;
    }

    /// <summary>
    /// Summary projection describing a delivered notification.
    /// </summary>
    public record NotificationSummary(Guid NotificationId, string Type, string Title, string ReferenceId, bool IsRead, DateTimeOffset CreatedAtUtc);

    /// <summary>
    /// Response payload containing paginated notification summaries.
    /// </summary>
    public record NotificationListResponse(int Page, int PageSize, IEnumerable<NotificationSummary> Notifications);

    /// <summary>
    /// Request payload listing notification identifiers to mark as read.
    /// </summary>
    public record MarkNotificationsReadRequest(IEnumerable<Guid> NotificationIds);

    /// <summary>
    /// Response payload confirming notifications were marked as read.
    /// </summary>
    public record MarkNotificationsReadResponse(IEnumerable<Guid> NotificationIds, DateTimeOffset MarkedAtUtc);

    /// <summary>
    /// Request payload specifying updated notification channel preferences.
    /// </summary>
    public record UpdateNotificationSettingsRequest(bool Email, bool Sms, bool Push);

    /// <summary>
    /// Response payload describing saved notification channel preferences.
    /// </summary>
    public record NotificationSettingsResponse(bool Email, bool Sms, bool Push, DateTimeOffset UpdatedAtUtc);
}
