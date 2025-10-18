using System;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Alerts;

public static class AlertEndpoints
{
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

    public record AlertSubscriptionRequest(string? Keyword, Guid? CategoryId, Guid? BrandId);

    public record AlertSubscriptionResponse(Guid AlertId, string? Keyword, Guid? CategoryId, Guid? BrandId, DateTimeOffset CreatedAtUtc);

    public record AlertDeletedResponse(Guid AlertId, DateTimeOffset DeletedAtUtc);
}
