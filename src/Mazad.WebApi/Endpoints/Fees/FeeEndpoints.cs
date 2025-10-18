using System;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Fees;

public static class FeeEndpoints
{
    public static RouteGroupBuilder MapFeeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/fees")
            .RequireAuthorization("Scope:mazad.api");

        group.MapGet("/estimate", ([FromQuery] FeeEstimateRequest request) =>
        {
            var estimate = new FeeEstimateResponse(request.ListingId, request.Amount, Math.Round(request.Amount * 0.07m, 2), "standard");
            return Results.Ok(estimate);
        });

        return group;
    }

    public record FeeEstimateRequest(Guid ListingId, decimal Amount);

    public record FeeEstimateResponse(Guid ListingId, decimal Amount, decimal Fee, string Tier);
}
