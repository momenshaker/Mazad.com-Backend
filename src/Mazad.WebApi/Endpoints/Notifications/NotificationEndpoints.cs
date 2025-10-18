using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Notifications;

public static class NotificationEndpoints
{
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

    public record NotificationSummary(Guid NotificationId, string Type, string Title, string ReferenceId, bool IsRead, DateTimeOffset CreatedAtUtc);

    public record NotificationListResponse(int Page, int PageSize, IEnumerable<NotificationSummary> Notifications);

    public record MarkNotificationsReadRequest(IEnumerable<Guid> NotificationIds);

    public record MarkNotificationsReadResponse(IEnumerable<Guid> NotificationIds, DateTimeOffset MarkedAtUtc);

    public record UpdateNotificationSettingsRequest(bool Email, bool Sms, bool Push);

    public record NotificationSettingsResponse(bool Email, bool Sms, bool Push, DateTimeOffset UpdatedAtUtc);
}
