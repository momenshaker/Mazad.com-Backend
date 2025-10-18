using System;
using System.Collections.Generic;

namespace Mazad.WebApi.Endpoints.Cms;

/// <summary>
/// Provides extension methods for CMS content delivery endpoints.
/// </summary>
public static class CmsEndpoints
{
    /// <summary>
    /// Maps endpoints used to retrieve CMS pages and content blocks.
    /// </summary>
    public static RouteGroupBuilder MapCmsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/cms");

        group.MapGet("/pages/{slug}", (string slug) =>
        {
            var page = new CmsPageResponse(slug, "Mazad Landing", "<h1>Welcome to Mazad</h1>", new Dictionary<string, string>
            {
                ["title"] = "Mazad | Premium Vehicle Auctions",
                ["description"] = "Bid on curated vehicles across the GCC."
            });

            return Results.Ok(page);
        });

        group.MapGet("/blocks/{blockKey}", (string blockKey) =>
        {
            var block = new CmsBlockResponse(blockKey, new { heading = "Featured Auctions", subtitle = "Ending soon" });
            return Results.Ok(block);
        });

        return group;
    }

    /// <summary>
    /// Response payload describing a CMS page with SEO metadata.
    /// </summary>
    public record CmsPageResponse(string Slug, string Title, string Html, IDictionary<string, string> Seo);

    /// <summary>
    /// Response payload describing a CMS content block.
    /// </summary>
    public record CmsBlockResponse(string Key, object Content);
}
