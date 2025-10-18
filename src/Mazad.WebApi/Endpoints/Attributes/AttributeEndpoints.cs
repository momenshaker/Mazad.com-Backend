using System;
using System.Security.Claims;
using Mazad.Application.Attributes.Commands;
using Mazad.Application.Attributes.Queries;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Attributes;

public static class AttributeEndpoints
{
    public static RouteGroupBuilder MapAttributeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/attributes");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] Guid? categoryId) =>
        {
            var result = await mediator.Send(new GetAttributeDefinitionsQuery(categoryId));
            return Results.Ok(result);
        });

        group.MapPost("/", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] UpsertAttributeDefinitionRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new CreateAttributeDefinitionCommand(request.CategoryId, request.Key, request.DisplayName, request.DataType, request.OptionsJson));
            return Results.Created($"/api/v1/attributes/{result.Id}", result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapPut("/{id:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] UpdateAttributeDefinitionRequest request) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new UpdateAttributeDefinitionCommand(id, request.DisplayName, request.DataType, request.OptionsJson));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapDelete("/{id:guid}", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id) =>
        {
            if (!user.HasScope("mazad.admin"))
            {
                return Results.Forbid();
            }

            await mediator.Send(new DeleteAttributeDefinitionCommand(id));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.admin");

        return group;
    }

    public record UpsertAttributeDefinitionRequest(Guid CategoryId, string Key, string DisplayName, string DataType, string? OptionsJson);
    public record UpdateAttributeDefinitionRequest(string DisplayName, string DataType, string? OptionsJson);
}
