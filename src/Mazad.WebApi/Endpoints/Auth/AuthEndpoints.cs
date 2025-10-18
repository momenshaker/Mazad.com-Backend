using System;
using System.Security.Claims;
using Mazad.Application.Auth.Commands;
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

        group.MapPost("/signup", async ([FromServices] IMediator mediator, [FromBody] SignupRequest request) =>
        {
            var result = await mediator.Send(new RegisterUserCommand(request.Email, request.Password, request.FullName));
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { result.Errors });
            }

            return Results.Created($"/api/v1/accounts/{result.UserId}", new { result.UserId, result.Email });
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

        group.MapPost("/set-password", async ([FromServices] IMediator mediator, ClaimsPrincipal user, [FromBody] SetPasswordRequest request) =>
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

    public record SignupRequest(string Email, string Password, string? FullName);

    public record LoginRequest(string Email, string Password, bool RememberMe);

    public record ForgotPasswordRequest(string Email);

    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public record SetPasswordRequest(string NewPassword, string? CurrentPassword);

    public record EnableMfaRequest(string? Code);
}
