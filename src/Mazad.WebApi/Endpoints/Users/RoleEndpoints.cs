using Mazad.Application.Users.Commands;
using Mazad.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Users;

/// <summary>
/// Provides extension methods for role management endpoints.
/// </summary>
public static class RoleEndpoints
{
    /// <summary>
    /// Maps endpoints for listing, creating, and deleting roles.
    /// </summary>
    public static RouteGroupBuilder MapRoleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/roles")
            .RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRolesQuery());
            return Results.Ok(result);
        });

        group.MapPost("/", async ([FromServices] IMediator mediator, [FromBody] RoleRequest request) =>
        {
            await mediator.Send(new CreateRoleCommand(request.RoleName));
            return Results.NoContent();
        });

        group.MapDelete("/{roleName}", async ([FromServices] IMediator mediator, string roleName) =>
        {
            await mediator.Send(new DeleteRoleCommand(roleName));
            return Results.NoContent();
        });

        return group;
    }

    /// <summary>
    /// Request payload used when creating a new role.
    /// </summary>
    public record RoleRequest(string RoleName);
}
