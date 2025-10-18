using System;
using System.Collections.Generic;

namespace Mazad.WebApi.Endpoints.Cms;

public static class CmsEndpoints
{
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

    public record CmsPageResponse(string Slug, string Title, string Html, IDictionary<string, string> Seo);

    public record CmsBlockResponse(string Key, object Content);
}
