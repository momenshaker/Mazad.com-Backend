using System.Security.Claims;
using Mazad.Application.Bids.Queries.GetBidById;
using Mazad.Application.Bids.Queries.GetMyBids;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Bids;

/// <summary>
/// Provides extension methods for bid-related endpoints.
/// </summary>
public static class BidEndpoints
{
    /// <summary>
    /// Maps endpoints for retrieving bidder activity and bid details.
    /// </summary>
    public static RouteGroupBuilder MapBidEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/bids");

        group.MapGet("/mine", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new GetMyBidsQuery(userId, page, pageSize));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.bidder");

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var userId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var result = await mediator.Send(new GetBidByIdQuery(id, userId, isAdmin));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.api");

        return group;
    }
}
