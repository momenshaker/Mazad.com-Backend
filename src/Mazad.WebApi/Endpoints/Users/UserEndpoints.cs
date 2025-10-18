using System;
using System.Collections.Generic;
using System.Security.Claims;
using Mazad.Application.Users.Commands;
using Mazad.Application.Users.Queries;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Users;

/// <summary>
/// Provides extension methods for user administration endpoints.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Maps endpoints for listing, updating, and moderating user accounts.
    /// </summary>
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/users");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] string? search, [FromQuery] string? role, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetUsersQuery(search, role, page, pageSize));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetUserByIdQuery(id));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapPut("/{id:guid}", async ([FromServices] IMediator mediator, Guid id, [FromBody] UpdateUserRequest request) =>
        {
            var result = await mediator.Send(new UpdateUserCommand(id, request.FullName, request.PhoneNumber, request.IsActive, request.IsDeleted, request.KycStatus, request.TwoFactorEnabled, request.Roles));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapDelete("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            await mediator.Send(new DeleteUserCommand(id));
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapGet("/{id:guid}/ratings", async ([FromServices] IMediator mediator, Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetUserRatingsQuery(id, page, pageSize));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.admin");

        group.MapPost("/{id:guid}/avatar", async ([FromServices] IMediator mediator, ClaimsPrincipal user, Guid id, [FromBody] UpdateAvatarRequest request) =>
        {
            if (!user.HasScope("mazad.admin") && user.GetUserId() != id)
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new UpdateUserAvatarCommand(id, request.AvatarUrl));
            return Results.Ok(result);
        }).RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Request payload for updating administrative user details.
    /// </summary>
    public record UpdateUserRequest(string? FullName, string? PhoneNumber, bool? IsActive, bool? IsDeleted, Mazad.Domain.Enums.KycStatus? KycStatus, bool? TwoFactorEnabled, IReadOnlyCollection<string>? Roles);
    /// <summary>
    /// Request payload for updating a user's avatar image.
    /// </summary>
    public record UpdateAvatarRequest(string AvatarUrl);
}
