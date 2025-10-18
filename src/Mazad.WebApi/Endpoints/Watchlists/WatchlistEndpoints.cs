using System.Security.Claims;
using Mazad.Application.Watchlists.Commands;
using Mazad.Application.Watchlists.Queries;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Watchlists;

/// <summary>
/// Provides extension methods for bidder watchlist endpoints.
/// </summary>
public static class WatchlistEndpoints
{
    /// <summary>
    /// Maps endpoints for managing the authenticated user's watchlist.
    /// </summary>
    public static RouteGroupBuilder MapWatchlistEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/watchlist")
            .RequireAuthorization("Scope:mazad.bidder");

        group.MapGet("/", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            var result = await mediator.Send(new GetMyWatchlistQuery(userId, page, pageSize));
            return Results.Ok(result);
        });

        group.MapPost("/{listingId:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid listingId) =>
        {
            var userId = user.GetUserId();
            await mediator.Send(new AddToWatchlistCommand(userId, listingId));
            return Results.NoContent();
        });

        group.MapDelete("/{listingId:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid listingId) =>
        {
            var userId = user.GetUserId();
            await mediator.Send(new RemoveFromWatchlistCommand(userId, listingId));
            return Results.NoContent();
        });

        return group;
    }
}
