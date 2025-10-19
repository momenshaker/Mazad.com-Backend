# Mazad.com Backend

This repository contains the initial backend foundation for Mazad.com built with **.NET 8**, **Entity Framework Core**, **ASP.NET Core Identity**, and **OpenIddict** following an Onion architecture.

## Solution structure

```
src/
  Mazad.Domain/            // Domain entities, enums, base types
  Mazad.Application/       // CQRS handlers, DTOs, validators
  Mazad.Infrastructure/    // EF Core DbContext, Identity, OpenIddict configuration
  Mazad.WebApi/            // Minimal API endpoints and composition root
Mazad.sln                  // Solution file referencing all projects
```

## Highlights

- Domain model covering categories, listings, bids, watchlists, orders, and CMS resources.
- Application layer with MediatR-based commands/queries for core workflows (seller CRUD/status transitions, moderation, browsing, bidding, watchlists, category administration).
- Infrastructure layer wiring EF Core SQL Server provider, ASP.NET Core Identity, and OpenIddict with scope-based policies.
- Minimal API endpoints for public, seller, and admin surfaces aligned with the MVP specification (listings, categories, bids, watchlists).
- Swagger/OpenAPI enabled for interactive exploration and testing.

## Business logic overview

Mazad.com centers on a multi-role auction marketplace. The core business rules are captured in the application layer as MediatR
handlers and enforced through request validators and policies:

- **Roles & scopes:** Admin, Seller, Bidder, and CMS users are authenticated through OpenIddict and authorized with scope-based policies to guarantee feature isolation (e.g., `Scope:mazad.seller` for seller endpoints).
- **Listing lifecycle:** Listings created by sellers progress through `Draft → PendingReview → Approved/Rejected → Active`, and can later transition to `Paused`, `Sold`, `Expired`, or `Cancelled` based on moderation outcomes, time windows, or business triggers.
- **Bidding rules:** Bids are accepted on active auction listings, the highest valid bid is marked as `Winning`, and outbid participants receive status updates that downstream jobs can process for notifications.
- **Watchlists & engagement:** Users can follow listings, increasing watch counts and enabling tailored experiences.
- **Moderation & CMS:** Admins and CMS staff manage catalog taxonomies, content pages, menus, and media assets while maintaining audit trails for compliance.

These rules are reflected in the domain entities (`Listing`, `Bid`, `Watchlist`, `Page`, etc.) and orchestrated via CQRS commands/queries that encapsulate the marketplace workflows.

## Getting started

1. Update the connection string in `src/Mazad.WebApi/appsettings.json` to point to your SQL Server instance.
2. Apply Entity Framework Core migrations (to be added) and run the Web API:

   ```bash
   dotnet build Mazad.sln
   dotnet run --project src/Mazad.WebApi/Mazad.WebApi.csproj
   ```

3. Navigate to `https://localhost:5001/swagger` to explore the endpoints and authenticate using OAuth scopes.

Further enhancements will add migrations, background jobs, full identity flows, testing projects, and additional endpoints listed in the delivery specification.
