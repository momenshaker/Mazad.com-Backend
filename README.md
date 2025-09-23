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

## Getting started

1. Update the connection string in `src/Mazad.WebApi/appsettings.json` to point to your SQL Server instance.
2. Apply Entity Framework Core migrations (to be added) and run the Web API:

   ```bash
   dotnet build Mazad.sln
   dotnet run --project src/Mazad.WebApi/Mazad.WebApi.csproj
   ```

3. Navigate to `https://localhost:5001/swagger` to explore the endpoints and authenticate using OAuth scopes.

Further enhancements will add migrations, background jobs, full identity flows, testing projects, and additional endpoints listed in the delivery specification.
