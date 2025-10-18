using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Orders;

/// <summary>
/// Provides extension methods for order management endpoints.
/// </summary>
public static class OrderEndpoints
{
    /// <summary>
    /// Maps endpoints used to view, create, and update orders.
    /// </summary>
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

    /// <summary>
    /// Summary projection describing an order for list views.
    /// </summary>
    public record OrderSummary(Guid OrderId, Guid ListingId, Guid CounterpartyId, decimal Total, string Status, DateTimeOffset UpdatedAtUtc);

    /// <summary>
    /// Response payload containing paginated order summaries.
    /// </summary>
    public record OrderListResponse(int Page, int PageSize, string Role, IEnumerable<OrderSummary> Orders);

    /// <summary>
    /// Request payload for creating a new order.
    /// </summary>
    public record CreateOrderRequest(Guid ListingId, Guid BuyerId, Guid SellerId, decimal Price);

    /// <summary>
    /// Detailed response payload describing an individual order.
    /// </summary>
    public record OrderDetails(Guid OrderId, Guid ListingId, Guid BuyerId, Guid SellerId, decimal Price, string Status, DateTimeOffset CreatedAtUtc);

    /// <summary>
    /// Request payload specifying a new status for an order.
    /// </summary>
    public record UpdateOrderStatusRequest(string Status);

    /// <summary>
    /// Response payload confirming an order status update.
    /// </summary>
    public record OrderStatusResponse(Guid OrderId, string Status, DateTimeOffset UpdatedAtUtc);

    /// <summary>
    /// Request payload to initiate payment for an order.
    /// </summary>
    public record PayOrderRequest(Guid PaymentMethodId, decimal Amount);

    /// <summary>
    /// Response payload describing the outcome of an order payment.
    /// </summary>
    public record OrderPaymentResponse(Guid OrderId, Guid PaymentMethodId, string Status, DateTimeOffset ProcessedAtUtc);

    /// <summary>
    /// Request payload to initiate an order refund.
    /// </summary>
    public record RefundOrderRequest(decimal Amount, string Reason);

    /// <summary>
    /// Response payload describing the outcome of an order refund.
    /// </summary>
    public record OrderRefundResponse(Guid OrderId, decimal Amount, string Status, DateTimeOffset ProcessedAtUtc);

    /// <summary>
    /// Entry describing a notable event in an order's lifecycle.
    /// </summary>
    public record OrderTimelineEntry(string Status, DateTimeOffset OccurredAtUtc, string Notes);

    /// <summary>
    /// Response payload containing the chronological order timeline.
    /// </summary>
    public record OrderTimelineResponse(Guid OrderId, IEnumerable<OrderTimelineEntry> Events);
}
