using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Payments;

public static class PaymentEndpoints
{
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

    public record PaymentMethodResponse(Guid MethodId, string Brand, string MaskedPan, bool IsDefault, DateTimeOffset Expiry);

    public record PaymentMethodsResponse(IEnumerable<PaymentMethodResponse> Methods);

    public record CreatePaymentMethodRequest(string Token, string Brand, string MaskedPan, bool IsDefault = false);

    public record CheckoutRequest(Guid ListingId, Guid? PaymentMethodId, decimal Amount);

    public record CheckoutSessionResponse(Guid SessionId, Guid ListingId, decimal Amount, string Status, string RedirectUrl);

    public record ConfirmPaymentRequest(Guid TransactionId, string Status);

    public record PaymentConfirmationResponse(Guid TransactionId, string Status, DateTimeOffset ProcessedAtUtc);

    public record PaymentTransactionResponse(Guid TransactionId, string Type, decimal Amount, string Status, DateTimeOffset UpdatedAtUtc);
}
