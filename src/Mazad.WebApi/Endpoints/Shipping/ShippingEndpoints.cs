using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Shipping;

public static class ShippingEndpoints
{
    public static void MapShippingEndpoints(this IEndpointRouteBuilder routes)
    {
        var shippingGroup = routes.MapGroup("/api/v1/shipping")
            .RequireAuthorization("Scope:mazad.api");

        shippingGroup.MapGet("/options", ([AsParameters] ShippingOptionsRequest request) =>
        {
            var options = new[]
            {
                new ShippingOptionResponse("Aramex", "express", 450m, DateTimeOffset.UtcNow.AddDays(3)),
                new ShippingOptionResponse("SMSA", "economy", 280m, DateTimeOffset.UtcNow.AddDays(6))
            };

            return Results.Ok(new ShippingOptionsResponse(options));
        });

        var orderShipping = routes.MapGroup("/api/v1/orders/{orderId:guid}/shipping")
            .RequireAuthorization("Scope:mazad.api");

        orderShipping.MapPost("/label", (Guid orderId, [FromBody] PurchaseLabelRequest request) =>
        {
            var label = new ShippingLabelResponse(orderId, Guid.NewGuid(), request.Carrier, request.ServiceLevel, "label-123.pdf", DateTimeOffset.UtcNow);
            return Results.Ok(label);
        });

        orderShipping.MapGet("/label", (Guid orderId) =>
        {
            var label = new ShippingLabelResponse(orderId, Guid.NewGuid(), "Aramex", "express", "label-123.pdf", DateTimeOffset.UtcNow.AddHours(-1));
            return Results.Ok(label);
        });

        orderShipping.MapPost("/tracking", (Guid orderId, [FromBody] AddTrackingRequest request) =>
        {
            var tracking = new TrackingResponse(orderId, request.TrackingNumber, request.Carrier, DateTimeOffset.UtcNow);
            return Results.Ok(tracking);
        });

        orderShipping.MapGet("/tracking", (Guid orderId) =>
        {
            var updates = new[]
            {
                new TrackingEvent("Picked Up", DateTimeOffset.UtcNow.AddHours(-12), "Jeddah depot"),
                new TrackingEvent("In Transit", DateTimeOffset.UtcNow.AddHours(-4), "Riyadh hub")
            };

            return Results.Ok(new TrackingHistoryResponse(orderId, updates));
        });
    }

    public record ShippingOptionsRequest(Guid ListingId, string DestinationCountry, string DestinationCity, decimal Weight, decimal Length, decimal Width, decimal Height);

    public record ShippingOptionResponse(string Carrier, string ServiceLevel, decimal Cost, DateTimeOffset EstimatedDeliveryDateUtc);

    public record ShippingOptionsResponse(IEnumerable<ShippingOptionResponse> Options);

    public record PurchaseLabelRequest(string Carrier, string ServiceLevel, string RecipientName, string AddressLine1, string City, string Country, string PostalCode);

    public record ShippingLabelResponse(Guid OrderId, Guid LabelId, string Carrier, string ServiceLevel, string LabelUrl, DateTimeOffset PurchasedAtUtc);

    public record AddTrackingRequest(string TrackingNumber, string Carrier);

    public record TrackingResponse(Guid OrderId, string TrackingNumber, string Carrier, DateTimeOffset AddedAtUtc);

    public record TrackingEvent(string Status, DateTimeOffset OccurredAtUtc, string Location);

    public record TrackingHistoryResponse(Guid OrderId, IEnumerable<TrackingEvent> Events);
}
