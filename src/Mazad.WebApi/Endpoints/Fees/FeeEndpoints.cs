using System;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Fees;

/// <summary>
/// Provides extension methods for fee calculation endpoints.
/// </summary>
public static class FeeEndpoints
{
    /// <summary>
    /// Maps endpoints used to estimate marketplace fees.
    /// </summary>
    public static RouteGroupBuilder MapFeeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/fees")
            .RequireAuthorization("Scope:mazad.api");

        group.MapGet("/estimate", ([FromQuery] Guid listingId, [FromQuery] decimal amount) =>
        {
            var request = new FeeEstimateRequest(listingId, amount);
            var estimate = new FeeEstimateResponse(request.ListingId, request.Amount, Math.Round(request.Amount * 0.07m, 2), "standard");
            return Results.Ok(estimate);
        });

        return group;
    }

    /// <summary>
    /// Request payload describing the parameters needed to estimate fees.
    /// </summary>
    public record FeeEstimateRequest(Guid ListingId, decimal Amount);

    /// <summary>
    /// Response payload containing the calculated fee information.
    /// </summary>
    public record FeeEstimateResponse(Guid ListingId, decimal Amount, decimal Fee, string Tier);
}
