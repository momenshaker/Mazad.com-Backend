using System.Security.Claims;
using Mazad.Application.Listings.Commands.Moderation;
using Mazad.Application.Listings.Queries.GetAdminListings;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Listings;

public static class AdminListingsEndpoints
{
    public static RouteGroupBuilder MapAdminListingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/listings")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] string? q, [FromQuery] Guid? sellerId, [FromQuery] Guid? categoryId, [FromQuery] string? status, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            Mazad.Domain.Enums.ListingStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Mazad.Domain.Enums.ListingStatus>(status, true, out var listingStatus))
            {
                parsedStatus = listingStatus;
            }

            var result = await mediator.Send(new GetAdminListingsQuery(q, sellerId, categoryId, parsedStatus, sort, page, pageSize));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/approve", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] ApproveListingRequest request) =>
        {
            var adminId = user.GetUserId();
            var result = await mediator.Send(new ApproveListingCommand(id, adminId, request.Notes));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/reject", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] RejectListingRequest request) =>
        {
            var adminId = user.GetUserId();
            var result = await mediator.Send(new RejectListingCommand(id, adminId, request.Reason));
            return Results.Ok(result);
        });

        return group;
    }

    public record ApproveListingRequest(string? Notes);
    public record RejectListingRequest(string Reason);
}
