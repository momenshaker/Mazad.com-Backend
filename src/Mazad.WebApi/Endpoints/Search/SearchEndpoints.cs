using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Search;

/// <summary>
/// Provides extension methods for marketplace search endpoints.
/// </summary>
public static class SearchEndpoints
{
    /// <summary>
    /// Maps endpoints for listing search, suggestions, and facets.
    /// </summary>
    public static RouteGroupBuilder MapSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/search");

        group.MapGet("/", ([AsParameters] SearchListingsRequest request) =>
        {
            var results = new[]
            {
                new SearchResult(Guid.NewGuid(), "2020 Toyota Camry", "Sedans", 82000m, DateTimeOffset.UtcNow.AddDays(1)),
                new SearchResult(Guid.NewGuid(), "2019 Nissan Patrol", "SUVs", 145000m, DateTimeOffset.UtcNow.AddHours(12))
            };

            return Results.Ok(new SearchResultsResponse(request.Page, request.PageSize, results));
        });

        group.MapGet("/suggestions", ([FromQuery] string q) =>
        {
            var suggestions = new[] { $"{q} premium", $"{q} certified", $"{q} low mileage" };
            return Results.Ok(new SearchSuggestionsResponse(q, suggestions));
        });

        group.MapGet("/facets", () =>
        {
            var facets = new SearchFacetsResponse(
                new Dictionary<string, IEnumerable<string>>
                {
                    ["category"] = new[] { "SUV", "Sedan", "Truck" },
                    ["brand"] = new[] { "Toyota", "Nissan", "Ford" },
                    ["priceRanges"] = new[] { "0-50000", "50000-100000", "100000+" }
                },
                new[]
                {
                    new EndingSoonResult(Guid.NewGuid(), "Lexus LX 570", DateTimeOffset.UtcNow.AddMinutes(45))
                });

            return Results.Ok(facets);
        });

        return group;
    }

    /// <summary>
    /// Request payload describing listing search filters.
    /// </summary>
    public record SearchListingsRequest(string? Q, Guid? CategoryId, Guid? BrandId, decimal? MinPrice, decimal? MaxPrice, string? Sort, int Page = 1, int PageSize = 20);

    /// <summary>
    /// Search result projection describing a matching listing.
    /// </summary>
    public record SearchResult(Guid ListingId, string Title, string Category, decimal CurrentPrice, DateTimeOffset EndsAtUtc);

    /// <summary>
    /// Search result projection describing a matching listing.
    /// </summary>
    /// <summary>
    /// Response payload containing paginated search results.
    /// </summary>
    public record SearchResultsResponse(int Page, int PageSize, IEnumerable<SearchResult> Listings);

    /// <summary>
    /// Response payload with search suggestions for a query.
    /// </summary>
    public record SearchSuggestionsResponse(string Query, IEnumerable<string> Suggestions);

    /// <summary>
    /// Projection describing a listing that is ending soon.
    /// </summary>
    public record EndingSoonResult(Guid ListingId, string Title, DateTimeOffset EndsAtUtc);

    /// <summary>
    /// Response payload describing available search facets and highlights.
    /// </summary>
    public record SearchFacetsResponse(IDictionary<string, IEnumerable<string>> Facets, IEnumerable<EndingSoonResult> EndingSoon);
}
