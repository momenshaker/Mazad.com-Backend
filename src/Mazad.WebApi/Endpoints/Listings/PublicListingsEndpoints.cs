using System.Security.Claims;
using Mazad.Application.Bids.Commands;
using Mazad.Application.Bids.Queries.GetListingBids;
using Mazad.Application.Listings.Queries.GetById;
using Mazad.Application.Listings.Queries.GetPublicListings;
using Mazad.Domain.Enums;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Listings;

public static class PublicListingsEndpoints
{
    public static RouteGroupBuilder MapPublicListingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/listings");

        group.MapGet("/", async (
            [FromServices] IMediator mediator,
            [FromQuery] string? q,
            [FromQuery] Guid? categoryId,
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] Guid? sellerId,
            [FromQuery] decimal? priceMin,
            [FromQuery] decimal? priceMax,
            [FromQuery] bool? endingSoon,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            ListingType? listingType = Enum.TryParse<Mazad.Domain.Enums.ListingType>(type, true, out var parsedType) ? parsedType : null;
            ListingStatus? listingStatus = Enum.TryParse<Mazad.Domain.Enums.ListingStatus>(status, true, out var parsedStatus) ? parsedStatus : null;

            var result = await mediator.Send(new GetPublicListingsQuery(
                q,
                categoryId,
                listingType,
                listingStatus,
                sellerId,
                priceMin,
                priceMax,
                endingSoon ?? false,
                sort,
                page,
                pageSize));

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetListingByIdQuery(id));
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}/bids", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user,
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var viewerId = user.GetUserId();
            var isAdmin = user.HasScope("mazad.admin");
            var result = await mediator.Send(new GetListingBidsQuery(id, viewerId, isAdmin, page, pageSize));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/bids", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] PlaceBidRequest request) =>
        {
            var bidderId = user.GetUserId();
            var bidId = await mediator.Send(new PlaceBidCommand(id, bidderId, request.Amount));
            return Results.Created($"/api/v1/bids/{bidId}", new { id = bidId });
        }).RequireAuthorization("Scope:mazad.bidder");

        return group;
    }

    public record PlaceBidRequest(decimal Amount);
}
