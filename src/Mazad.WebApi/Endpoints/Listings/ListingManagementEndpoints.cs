using System;
using System.Security.Claims;
using Mazad.Application.Listings.Commands.Create;
using Mazad.Application.Listings.Commands.Delete;
using Mazad.Application.Listings.Commands.Media;
using Mazad.Application.Listings.Commands.Publishing;
using Mazad.Application.Listings.Commands.Update;
using Mazad.Application.Listings.Queries.GetHistory;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Listings;

/// <summary>
/// Provides extension methods for comprehensive listing management endpoints.
/// </summary>
public static class ListingManagementEndpoints
{
    /// <summary>
    /// Maps endpoints used to create, update, and administrate listings.
    /// </summary>
    public static RouteGroupBuilder MapListingManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/listings");

        group.MapPost("/", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            [FromBody] CreateListingCommand command) =>
        {
            var sellerId = user.GetUserId();
            var result = await mediator.Send(command with { SellerId = sellerId });
            return Results.Created($"/api/v1/listings/{result.Id}", result);
        }).RequireAuthorization("Scope:mazad.seller");

        group.MapPut("/{id:guid}", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id,
            [FromBody] UpdateListingRequest request) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var command = new UpdateListingCommand
            {
                ListingId = id,
                ActorId = actorId,
                IsAdmin = isAdmin,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                Attributes = request.Attributes,
                Type = request.Type,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                StartPrice = request.StartPrice,
                ReservePrice = request.ReservePrice,
                BidIncrement = request.BidIncrement,
                BuyNowPrice = request.BuyNowPrice
            };

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapDelete("/{id:guid}", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            await mediator.Send(new DeleteListingCommand(id, actorId, isAdmin));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapPost("/{id:guid}/images", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id,
            [FromBody] UploadListingImagesRequest request) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var command = new AddListingImagesCommand(id, actorId, isAdmin, request.Images);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapDelete("/{id:guid}/images/{imageId:guid}", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id,
            Guid imageId) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            await mediator.Send(new RemoveListingImageCommand(id, imageId, actorId, isAdmin));
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapPost("/{id:guid}/publish", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var result = await mediator.Send(new PublishListingCommand(id, actorId, isAdmin));
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapPost("/{id:guid}/unpublish", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var result = await mediator.Send(new UnpublishListingCommand(id, actorId, isAdmin));
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapPost("/{id:guid}/extend", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id,
            [FromBody] ExtendListingRequest request) =>
        {
            var actorId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var result = await mediator.Send(new ExtendListingCommand(id, actorId, isAdmin, request.NewEndAt));
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireAssertion(context =>
            context.User.HasScope("mazad.seller") || context.User.HasScope("mazad.admin")));

        group.MapPost("/{id:guid}/buy-now", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id) =>
        {
            var buyerId = user.GetUserId();
            var result = await mediator.Send(new BuyNowCommand(id, buyerId));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.bidder");

        group.MapGet("/{id:guid}/history", async (
            [FromServices] IMediator mediator,
            Guid id) =>
        {
            var result = await mediator.Send(new GetListingHistoryQuery(id));
            return Results.Ok(result);
        });

        return group;
    }

    /// <summary>
    /// Request payload containing listing details for update operations.
    /// </summary>
    public record UpdateListingRequest
    {
        public Guid? CategoryId { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Location { get; init; }
        public string? Attributes { get; init; }
        public Mazad.Domain.Enums.ListingType? Type { get; init; }
        public DateTimeOffset? StartAt { get; init; }
        public DateTimeOffset? EndAt { get; init; }
        public decimal? StartPrice { get; init; }
        public decimal? ReservePrice { get; init; }
        public decimal? BidIncrement { get; init; }
        public decimal? BuyNowPrice { get; init; }
    }

    /// <summary>
    /// Request payload for uploading listing media assets.
    /// </summary>
    public record UploadListingImagesRequest
    {
        public IReadOnlyCollection<AddListingImageRequest> Images { get; init; } = Array.Empty<AddListingImageRequest>();
    }

    /// <summary>
    /// Request payload for extending the duration of a listing.
    /// </summary>
    public record ExtendListingRequest(DateTimeOffset NewEndAt);
}
