using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Orders;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/orders")
            .RequireAuthorization("Scope:mazad.api");

        group.MapGet("/", ([FromQuery] string? role, [FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var orders = new[]
            {
                new OrderSummary(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 152000m, "Pending", DateTimeOffset.UtcNow.AddMinutes(-30))
            };

            return Results.Ok(new OrderListResponse(page, pageSize, role ?? "all", orders));
        });

        group.MapPost("/", ([FromBody] CreateOrderRequest request) =>
        {
            var order = new OrderDetails(Guid.NewGuid(), request.ListingId, request.BuyerId, request.SellerId, request.Price, "Pending", DateTimeOffset.UtcNow);
            return Results.Created($"/api/v1/orders/{order.OrderId}", order);
        });

        group.MapGet("/{orderId:guid}", (Guid orderId) =>
        {
            var order = new OrderDetails(orderId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 182500m, "Paid", DateTimeOffset.UtcNow.AddDays(-1));
            return Results.Ok(order);
        });

        group.MapPut("/{orderId:guid}/status", (Guid orderId, [FromBody] UpdateOrderStatusRequest request) =>
        {
            var updated = new OrderStatusResponse(orderId, request.Status, DateTimeOffset.UtcNow);
            return Results.Ok(updated);
        });

        group.MapPost("/{orderId:guid}/pay", (Guid orderId, [FromBody] PayOrderRequest request) =>
        {
            var payment = new OrderPaymentResponse(orderId, request.PaymentMethodId, "processing", DateTimeOffset.UtcNow);
            return Results.Ok(payment);
        });

        group.MapPost("/{orderId:guid}/refund", (Guid orderId, [FromBody] RefundOrderRequest request) =>
        {
            var refund = new OrderRefundResponse(orderId, request.Amount, "initiated", DateTimeOffset.UtcNow);
            return Results.Ok(refund);
        });

        group.MapGet("/{orderId:guid}/timeline", (Guid orderId) =>
        {
            var timeline = new[]
            {
                new OrderTimelineEntry("Created", DateTimeOffset.UtcNow.AddDays(-2), "Order created"),
                new OrderTimelineEntry("Paid", DateTimeOffset.UtcNow.AddDays(-1), "Payment confirmed"),
                new OrderTimelineEntry("Shipped", DateTimeOffset.UtcNow.AddHours(-6), "Carrier: Aramex, Tracking: XYZ123")
            };

            return Results.Ok(new OrderTimelineResponse(orderId, timeline));
        });

        return group;
    }

    public record OrderSummary(Guid OrderId, Guid ListingId, Guid CounterpartyId, decimal Total, string Status, DateTimeOffset UpdatedAtUtc);

    public record OrderListResponse(int Page, int PageSize, string Role, IEnumerable<OrderSummary> Orders);

    public record CreateOrderRequest(Guid ListingId, Guid BuyerId, Guid SellerId, decimal Price);

    public record OrderDetails(Guid OrderId, Guid ListingId, Guid BuyerId, Guid SellerId, decimal Price, string Status, DateTimeOffset CreatedAtUtc);

    public record UpdateOrderStatusRequest(string Status);

    public record OrderStatusResponse(Guid OrderId, string Status, DateTimeOffset UpdatedAtUtc);

    public record PayOrderRequest(Guid PaymentMethodId, decimal Amount);

    public record OrderPaymentResponse(Guid OrderId, Guid PaymentMethodId, string Status, DateTimeOffset ProcessedAtUtc);

    public record RefundOrderRequest(decimal Amount, string Reason);

    public record OrderRefundResponse(Guid OrderId, decimal Amount, string Status, DateTimeOffset ProcessedAtUtc);

    public record OrderTimelineEntry(string Status, DateTimeOffset OccurredAtUtc, string Notes);

    public record OrderTimelineResponse(Guid OrderId, IEnumerable<OrderTimelineEntry> Events);
}
