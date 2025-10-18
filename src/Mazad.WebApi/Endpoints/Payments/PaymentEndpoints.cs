using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Payments;

/// <summary>
/// Provides extension methods for mapping bidder payment endpoints.
/// </summary>
public static class PaymentEndpoints
{
    /// <summary>
    /// Maps payment endpoints for managing methods and processing transactions.
    /// </summary>
    public static RouteGroupBuilder MapPaymentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/payments")
            .RequireAuthorization("Scope:mazad.bidder");

        group.MapGet("/methods", () =>
        {
            var methods = new[]
            {
                new PaymentMethodResponse(Guid.NewGuid(), "visa", "•••• 4242", true, DateTimeOffset.UtcNow.AddYears(2)),
                new PaymentMethodResponse(Guid.NewGuid(), "mada", "•••• 1234", false, DateTimeOffset.UtcNow.AddYears(1))
            };

            return Results.Ok(new PaymentMethodsResponse(methods));
        });

        group.MapPost("/methods", ([FromBody] CreatePaymentMethodRequest request) =>
        {
            var method = new PaymentMethodResponse(Guid.NewGuid(), request.Brand, request.MaskedPan, request.IsDefault, DateTimeOffset.UtcNow.AddYears(4));
            return Results.Created($"/api/v1/payments/methods/{method.MethodId}", method);
        });

        group.MapDelete("/methods/{methodId:guid}", (Guid methodId) =>
        {
            return Results.NoContent();
        });

        group.MapPost("/checkout", ([FromBody] CheckoutRequest request) =>
        {
            var session = new CheckoutSessionResponse(Guid.NewGuid(), request.ListingId, request.Amount, "pending", "https://payments.example.com/checkout/123");
            return Results.Ok(session);
        });

        group.MapPost("/confirm", ([FromBody] ConfirmPaymentRequest request) =>
        {
            var confirmation = new PaymentConfirmationResponse(request.TransactionId, "confirmed", DateTimeOffset.UtcNow);
            return Results.Ok(confirmation);
        });

        group.MapGet("/transactions/{transactionId:guid}", (Guid transactionId) =>
        {
            var transaction = new PaymentTransactionResponse(transactionId, "order", 152000m, "settled", DateTimeOffset.UtcNow.AddMinutes(-10));
            return Results.Ok(transaction);
        });

        return group;
    }

    /// <summary>
    /// Response payload describing a stored payment method.
    /// </summary>
    public record PaymentMethodResponse(Guid MethodId, string Brand, string MaskedPan, bool IsDefault, DateTimeOffset Expiry);

    /// <summary>
    /// Response payload that wraps a collection of payment methods.
    /// </summary>
    public record PaymentMethodsResponse(IEnumerable<PaymentMethodResponse> Methods);

    /// <summary>
    /// Request payload for creating a new payment method.
    /// </summary>
    public record CreatePaymentMethodRequest(string Token, string Brand, string MaskedPan, bool IsDefault = false);

    /// <summary>
    /// Request payload used to initiate a checkout session.
    /// </summary>
    public record CheckoutRequest(Guid ListingId, Guid? PaymentMethodId, decimal Amount);

    /// <summary>
    /// Response payload describing an initiated checkout session.
    /// </summary>
    public record CheckoutSessionResponse(Guid SessionId, Guid ListingId, decimal Amount, string Status, string RedirectUrl);

    /// <summary>
    /// Request payload sent to confirm a payment transaction.
    /// </summary>
    public record ConfirmPaymentRequest(Guid TransactionId, string Status);

    /// <summary>
    /// Response payload returned after confirming a payment.
    /// </summary>
    public record PaymentConfirmationResponse(Guid TransactionId, string Status, DateTimeOffset ProcessedAtUtc);

    /// <summary>
    /// Response payload describing a historical payment transaction.
    /// </summary>
    public record PaymentTransactionResponse(Guid TransactionId, string Type, decimal Amount, string Status, DateTimeOffset UpdatedAtUtc);
}
