using System;
using System.Security.Claims;
using Mazad.Application.Accounts.Commands;
using Mazad.Application.Accounts.Queries;
using Mazad.Application.Auth.Commands;
using Mazad.Application.Common.Models;
using Mazad.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth");

        group.MapPost("/register", async ([FromServices] IMediator mediator, [FromBody] RegisterRequest request) =>
        {
            var result = await mediator.Send(new RegisterUserCommand(request.Email, request.Password, request.FullName));
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { result.Errors });
            }

            return Results.Created($"/api/v1/auth/profile", new { result.UserId, result.Email });
        }).AllowAnonymous();

        group.MapPost("/login", async ([FromServices] IMediator mediator, [FromBody] LoginRequest request) =>
        {
            var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password, request.RememberMe));
            if (result.Succeeded || result.RequiresTwoFactor)
            {
                return Results.Ok(result);
            }

            if (result.IsLockedOut)
            {
                return Results.Json(result, statusCode: StatusCodes.Status423Locked);
            }

            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }).AllowAnonymous();

        group.MapPost("/refresh", ([FromBody] RefreshTokenRequest request) =>
        {
            return Results.StatusCode(StatusCodes.Status501NotImplemented);
        }).AllowAnonymous();

        group.MapPost("/logout", async ([FromServices] IMediator mediator) =>
        {
            await mediator.Send(new LogoutUserCommand());
            return Results.NoContent();
        }).RequireAuthorization("Scope:mazad.api");

        group.MapPost("/forgot-password", async ([FromServices] IMediator mediator, [FromBody] ForgotPasswordRequest request) =>
        {
            var result = await mediator.Send(new ForgotPasswordCommand(request.Email));
            return Results.Ok(result);
        }).AllowAnonymous();

        group.MapPost("/reset-password", async ([FromServices] IMediator mediator, [FromBody] ResetPasswordRequest request) =>
        {
            var result = await mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword));
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { result.Errors });
            }

            return Results.Ok(result);
        }).AllowAnonymous();

        group.MapGet("/profile", async ([FromServices] IMediator mediator, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new GetCurrentAccountQuery(userId));
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.api");

        group.MapPut("/profile", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] UpdateProfileRequest request) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var profile = request.Profile is null
                ? null
                : new UpdateAccountProfileDto(
                    request.Profile.AvatarUrl,
                    request.Profile.Address,
                    request.Profile.City,
                    request.Profile.Country,
                    request.Profile.Language,
                    request.Profile.Timezone);

            var command = new UpdateAccountCommand(userId, request.FullName, request.PhoneNumber, profile);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.api");

        group.MapPut("/profile/password", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] UpdatePasswordRequest request) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new SetPasswordCommand(userId, request.NewPassword, request.CurrentPassword));
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { result.RequiresCurrentPassword, result.Errors });
            }

            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.api");

        group.MapPost("/verify-email", () => Results.StatusCode(StatusCodes.Status501NotImplemented))
            .RequireAuthorization("Scope:mazad.api");

        group.MapPost("/verify-phone", () => Results.StatusCode(StatusCodes.Status501NotImplemented))
            .RequireAuthorization("Scope:mazad.api");

        group.MapPost("/mfa/enable", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] EnableMfaRequest request) =>
        {
            var userId = user.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new EnableAuthenticatorMfaCommand(userId, request.Code));
            if (!result.Succeeded && !result.RequiresVerification)
            {
                return Results.BadRequest(new { result.Errors });
            }

            return Results.Ok(result);
        }).RequireAuthorization("Scope:mazad.api");

        return group;
    }

    public record RegisterRequest(string Email, string Password, string? FullName);

    public record LoginRequest(string Email, string Password, bool RememberMe);

    public record RefreshTokenRequest(string RefreshToken);

    public record ForgotPasswordRequest(string Email);

    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public record UpdateProfileRequest(string? FullName, string? PhoneNumber, UpdateProfileDetailsRequest? Profile);

    public record UpdateProfileDetailsRequest(string? AvatarUrl, string? Address, string? City, string? Country, string? Language, string? Timezone);

    public record UpdatePasswordRequest(string NewPassword, string? CurrentPassword);

    public record EnableMfaRequest(string? Code);
}
