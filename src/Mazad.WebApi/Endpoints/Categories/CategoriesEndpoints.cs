using Mazad.Application.Categories.Commands;
using Mazad.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Categories;

public static class CategoriesEndpoints
{
    public static RouteGroupBuilder MapPublicCategoriesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/categories");

        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCategoriesTreeQuery());
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetCategoryByIdQuery(id));
            return Results.Ok(result);
        });

        return group;
    }

    public static RouteGroupBuilder MapAdminCategoriesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/categories")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapPost("/", async ([FromServices] IMediator mediator, [FromBody] CreateCategoryRequest request) =>
        {
            var result = await mediator.Send(new CreateCategoryCommand(request.ParentId, request.Name, request.Slug, request.AttributesSchema));
            return Results.Created($"/api/v1/admin/categories/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async ([FromServices] IMediator mediator, Guid id, [FromBody] UpdateCategoryRequest request) =>
        {
            var result = await mediator.Send(new UpdateCategoryCommand(id, request.Name, request.Slug, request.AttributesSchema));
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            await mediator.Send(new DeleteCategoryCommand(id));
            return Results.NoContent();
        });

        return group;
    }

    public record CreateCategoryRequest(Guid? ParentId, string Name, string? Slug, string? AttributesSchema);
    public record UpdateCategoryRequest(string Name, string? Slug, string? AttributesSchema);
}
