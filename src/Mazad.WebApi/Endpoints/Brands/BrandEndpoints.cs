using System.Security.Claims;
using Mazad.Application.Brands.Commands;
using Mazad.Application.Brands.Queries;
using Mazad.Application.Models.Commands;
using Mazad.Application.Models.Queries;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Brands;

public static class BrandEndpoints
{
    public static RouteGroupBuilder MapBrandEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/brands");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] string? search) =>
        {
            var result = await mediator.Send(new GetVehicleBrandsQuery(search));
            return Results.Ok(result);
        });

        group.MapPost("/", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] CreateVehicleBrandRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new CreateVehicleBrandCommand(request.Name, request.Slug));
            return Results.Created($"/api/v1/brands/{result.Id}", result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapPut("/{id:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] UpdateVehicleBrandRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new UpdateVehicleBrandCommand(id, request.Name, request.Slug));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapDelete("/{id:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            await mediator.Send(new DeleteVehicleBrandCommand(id));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/{id:guid}/models", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetVehicleModelsByBrandQuery(id));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/models", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] CreateVehicleModelRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new CreateVehicleModelCommand(id, request.Name, request.Slug));
            return Results.Created($"/api/v1/models/{result.Id}", result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapPut("/models/{modelId:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid modelId, [FromBody] UpdateVehicleModelRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new UpdateVehicleModelCommand(modelId, request.Name, request.Slug));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapDelete("/models/{modelId:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid modelId) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            await mediator.Send(new DeleteVehicleModelCommand(modelId));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.admin");

        return group;
    }

    public record CreateVehicleBrandRequest(string Name, string? Slug);
    public record UpdateVehicleBrandRequest(string Name, string? Slug);
    public record CreateVehicleModelRequest(string Name, string? Slug);
    public record UpdateVehicleModelRequest(string Name, string? Slug);
}
