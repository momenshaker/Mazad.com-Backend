using System.Security.Claims;
using Mazad.Application.Accounts.Commands;
using Mazad.Application.Accounts.Queries;
using Mazad.Application.Common.Models;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Accounts;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/accounts")
            .RequireAuthorization("Scope:mazad.api");

        group.MapGet("/me", async ([FromServices] IMediator mediator, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new GetCurrentAccountQuery(userId));
            return Results.Ok(result);
        });

        group.MapPut("/me", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] UpdateAccountRequest request) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var command = new UpdateAccountCommand(
                userId,
                request.FullName,
                request.PhoneNumber,
                request.Profile is null
                    ? null
                    : new UpdateAccountProfileDto(
                        request.Profile.AvatarUrl,
                        request.Profile.Address,
                        request.Profile.City,
                        request.Profile.Country,
                        request.Profile.Language,
                        request.Profile.Timezone));

            var result = await mediator.Send(command);
            return Results.Ok(result);
        });

        return group;
    }

    public record UpdateAccountRequest(
        string? FullName,
        string? PhoneNumber,
        UpdateAccountProfileRequest? Profile);

    public record UpdateAccountProfileRequest(
        string? AvatarUrl,
        string? Address,
        string? City,
        string? Country,
        string? Language,
        string? Timezone);
}
