using System;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Alerts;

/// <summary>
/// Provides extension methods for bidder alert subscription endpoints.
/// </summary>
public static class AlertEndpoints
{
    /// <summary>
    /// Maps endpoints for managing saved search alerts.
    /// </summary>
    public static RouteGroupBuilder MapAlertEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/alerts")
            .RequireAuthorization("Scope:mazad.bidder");

        group.MapPost("/subscribe", ([AsParameters] AlertSubscriptionRequest request) =>
        {
            var alert = new AlertSubscriptionResponse(Guid.NewGuid(), request.Keyword, request.CategoryId, request.BrandId, DateTimeOffset.UtcNow);
            return Results.Ok(alert);
        });

        group.MapDelete("/{alertId:guid}", (Guid alertId) =>
        {
            return Results.Ok(new AlertDeletedResponse(alertId, DateTimeOffset.UtcNow));
        });

        return group;
    }

    /// <summary>
    /// Request payload describing the alert criteria a bidder wants to follow.
    /// </summary>
    public record AlertSubscriptionRequest(string? Keyword, Guid? CategoryId, Guid? BrandId);

    /// <summary>
    /// Response payload returned after creating an alert subscription.
    /// </summary>
    public record AlertSubscriptionResponse(Guid AlertId, string? Keyword, Guid? CategoryId, Guid? BrandId, DateTimeOffset CreatedAtUtc);

    /// <summary>
    /// Response payload returned after deleting an alert subscription.
    /// </summary>
    public record AlertDeletedResponse(Guid AlertId, DateTimeOffset DeletedAtUtc);
}
