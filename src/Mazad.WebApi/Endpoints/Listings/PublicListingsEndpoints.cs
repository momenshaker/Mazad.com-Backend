using System.Security.Claims;
using Mazad.Application.Bids.Commands;
using Mazad.Application.Listings.Queries.GetById;
using Mazad.Application.Listings.Queries.GetPublicListings;
using Mazad.Application.Watchlists.Commands;
using Mazad.Application.Watchlists.Queries;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Listings;

public static class PublicListingsEndpoints
{
    public static RouteGroupBuilder MapPublicListingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/listings");

        //group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] string? q, [FromQuery] Guid? categoryId, [FromQuery] string? type, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        //{
        //    var listingType = Enum.TryParse<Mazad.Domain.Enums.ListingType>(type, true, out var parsed) ? parsed : null;
        //    var result = await mediator.Send(new GetPublicListingsQuery(q, categoryId, listingType, sort, page, pageSize));
        //    return Results.Ok(result);
        //});

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetListingByIdQuery(id));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/bids", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] PlaceBidRequest request) =>
        {
            var bidderId = user.GetUserId();
            var bidId = await mediator.Send(new PlaceBidCommand(id, bidderId, request.Amount));
            return Results.Created($"/api/v1/bids/{bidId}", new { id = bidId });
        }).RequireAuthorization("Scope:mazad.bidder");

        group.MapGet("/watchlist", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            var result = await mediator.Send(new GetMyWatchlistQuery(userId, page, pageSize));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.bidder");

        group.MapPost("/{id:guid}/watch", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var userId = user.GetUserId();
            await mediator.Send(new AddToWatchlistCommand(userId, id));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.bidder");

        group.MapDelete("/{id:guid}/watch", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var userId = user.GetUserId();
            await mediator.Send(new RemoveFromWatchlistCommand(userId, id));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.bidder");

        return group;
    }

    public record PlaceBidRequest(decimal Amount);
}
