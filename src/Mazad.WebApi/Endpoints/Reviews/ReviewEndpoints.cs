using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Reviews;

/// <summary>
/// Provides extension methods for review submission and retrieval endpoints.
/// </summary>
public static class ReviewEndpoints
{
    /// <summary>
    /// Maps endpoints for creating and listing reviews.
    /// </summary>
    public static void MapReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/orders/{orderId:guid}/reviews", (Guid orderId, [FromBody] CreateReviewRequest request) =>
        {
            var review = new ReviewResponse(Guid.NewGuid(), orderId, request.Score, request.Comment, DateTimeOffset.UtcNow, "published");
            return Results.Created($"/api/v1/orders/{orderId}/reviews/{review.ReviewId}", review);
        }).RequireAuthorization("Scope:mazad.bidder");

        routes.MapGet("/api/v1/users/{userId:guid}/reviews", (Guid userId, [FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var reviews = new[]
            {
                new ReviewSummary(Guid.NewGuid(), userId, Guid.NewGuid(), 5, "Great seller", DateTimeOffset.UtcNow.AddDays(-10))
            };

            return Results.Ok(new ReviewListResponse(page, pageSize, reviews));
        });

        routes.MapGet("/api/v1/listings/{listingId:guid}/reviews", (Guid listingId, [FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var reviews = new[]
            {
                new ReviewSummary(Guid.NewGuid(), Guid.NewGuid(), listingId, 4, "Accurate description", DateTimeOffset.UtcNow.AddDays(-3))
            };

            return Results.Ok(new ReviewListResponse(page, pageSize, reviews));
        });
    }

    /// <summary>
    /// Request payload for submitting a review.
    /// </summary>
    public record CreateReviewRequest(int Score, string Comment);

    /// <summary>
    /// Response payload describing a submitted review.
    /// </summary>
    public record ReviewResponse(Guid ReviewId, Guid OrderId, int Score, string Comment, DateTimeOffset CreatedAtUtc, string Status);

    /// <summary>
    /// Summary projection describing a review for list views.
    /// </summary>
    public record ReviewSummary(Guid ReviewId, Guid ReviewerId, Guid SubjectId, int Score, string Comment, DateTimeOffset CreatedAtUtc);

    /// <summary>
    /// Response payload containing paginated review summaries.
    /// </summary>
    public record ReviewListResponse(int Page, int PageSize, IEnumerable<ReviewSummary> Reviews);
}
