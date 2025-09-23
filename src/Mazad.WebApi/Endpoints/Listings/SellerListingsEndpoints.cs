using System.Security.Claims;
using Mazad.Application.Listings.Commands.Create;
using Mazad.Application.Listings.Commands.Submit;
using Mazad.Application.Listings.Commands.UpdateStatus;
using Mazad.Application.Listings.Queries.GetSellerListings;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Listings;

public static class SellerListingsEndpoints
{
    public static RouteGroupBuilder MapSellerListingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/seller/listings")
            .RequireAuthorization("Scope:mazad.seller");

        group.MapGet("/", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(new GetSellerListingsQuery(sellerId, page, pageSize));
            return Results.Ok(result);
        });

        group.MapPost("/", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] CreateListingCommand command) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(command with { SellerId = sellerId });
            return Results.Created($"/api/v1/seller/listings/{result.Id}", result);
        });

        group.MapPost("/{id:guid}/submit", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(new SubmitListingCommand(id, sellerId));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/pause", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(new UpdateListingStatusCommand(id, sellerId, Mazad.Domain.Enums.ListingStatus.Paused));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/resume", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(new UpdateListingStatusCommand(id, sellerId, Mazad.Domain.Enums.ListingStatus.Active));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/cancel", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] CancelListingRequest request) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(new UpdateListingStatusCommand(id, sellerId, Mazad.Domain.Enums.ListingStatus.Cancelled, request.Reason));
            return Results.Ok(result);
        });

        return group;
    }

    public record CancelListingRequest(string? Reason);
}
