using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Search;

public static class SearchEndpoints
{
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

    public record SearchListingsRequest(string? Q, Guid? CategoryId, Guid? BrandId, decimal? MinPrice, decimal? MaxPrice, string? Sort, int Page = 1, int PageSize = 20);

    public record SearchResult(Guid ListingId, string Title, string Category, decimal CurrentPrice, DateTimeOffset EndsAtUtc);

    public record SearchResultsResponse(int Page, int PageSize, IEnumerable<SearchResult> Listings);

    public record SearchSuggestionsResponse(string Query, IEnumerable<string> Suggestions);

    public record EndingSoonResult(Guid ListingId, string Title, DateTimeOffset EndsAtUtc);

    public record SearchFacetsResponse(IDictionary<string, IEnumerable<string>> Facets, IEnumerable<EndingSoonResult> EndingSoon);
}
