Awesome — I’ve reworked your **AGENT.md** into a backend-first delivery spec for a **.NET 8 + MSSQL + EF Core (Code-First) + Onion Architecture** stack, with **OAuth2/OpenID Connect**, covering **Admin**, **Seller**, **Bidder** panels, and a **CMS** layer. It includes: architecture, domain model, statuses/workflows, CRUD + status endpoints (with request/response shapes), pagination/filtering conventions, auth scopes/roles, and bootstrapping notes.

---

# AGENT.md — Mazad.com Backend MVP (API + Auth + Data)

> Single source of truth for delivering Mazad.com’s **backend**: clean architecture, entities, workflows, and a complete REST API surface (CRUD + status transitions) for **Admin/Seller/Bidder** panels and **CMS**.

**Stack:** .NET 8, MSSQL, EF Core Code-First, FluentValidation, MediatR, OpenIddict (OAuth2/OIDC), Serilog.

---

## 1) Architecture

### 1.1 Onion Layers

```
src/
  Mazad.Domain/                 // Entities, Value Objects, Enums, Domain Events, Specs
  Mazad.Application/            // Use cases (CQRS with MediatR), DTOs, Validators, Policies
  Mazad.Infrastructure/         // EF Core, Repositories, Identity, OpenIddict, MSSQL, Email/SMS, FileStore
  Mazad.WebApi/                 // Presentation: Minimal APIs/Controllers, Filters, Auth, Versioning, Swagger
  Mazad.BackgroundJobs/         // Outbid emails, auction closing, reminders (Hangfire/Quartz) [P2]
tests/
  Mazad.UnitTests/
  Mazad.IntegrationTests/
```

### 1.2 Cross-cutting

* **Auth:** OpenIddict (Authorization Code + PKCE, Client Credentials, Refresh Tokens).
* **Identity:** ASP.NET Core Identity (Users/Roles/Claims).
* **Persistence:** EF Core (MSSQL), Migrations, Auditing (Created/UpdatedBy/At), Soft Delete.
* **Validation:** FluentValidation (request DTOs).
* **Observability:** Serilog + OpenTelemetry (optional).
* **Versioning:** URL `/api/v1/...`.
* **Conventions:** Pagination `?page=1&pageSize=20`, sorting `?sort=field,-other`, filtering `?q=...&status=...`.

---

## 2) Roles, Panels & Scopes

### 2.1 Roles

* **Admin**: full access, moderation, CMS, reports.
* **Seller**: manage own listings, orders, payouts, profile, store.
* **Bidder**: browse, follow, bid, checkout (P2+), profile, KYC (optional).
* **CMS\_Editor** / **CMS\_Admin**: pages, menus, media, translations.
* **Support**: read-only + limited actions (refunds/flags) \[P2].

### 2.2 OAuth2/OIDC

* **Authority:** `https://auth.mazad.com`
* **Clients:**

  * `admin-panel` (Auth Code + PKCE, scopes: `openid profile email roles mazad.admin mazad.cms mazad.api`)
  * `seller-panel` (Auth Code + PKCE, scopes: `openid profile email roles mazad.seller mazad.api`)
  * `bidder-panel` (Auth Code + PKCE, scopes: `openid profile email roles mazad.bidder mazad.api`)
  * `internal-jobs` (Client Credentials, scope: `mazad.jobs`)
* **Scopes/Resources:** `mazad.api`, `mazad.admin`, `mazad.seller`, `mazad.bidder`, `mazad.cms`, `offline_access`.

---

## 3) Core Domain (Entities & Statuses)

### 3.1 Users & Identity

* `User` (ASP.NET Identity): Id, Email, Phone, FullName, Roles\[], 2FAEnabled, KYCStatus (None|Pending|Verified|Rejected).
* `UserProfile`: FK UserId, AvatarUrl, Address, City, Country, Language, Timezone.

### 3.2 Catalog & Taxonomy

* `Category` (tree): Id, ParentId?, Slug, Name, AttributesSchema (JSON).
* `VehicleBrand`, `VehicleModel`, `VehicleTrim`, `YearRange`.
* `AttributeDefinition`: Schema for dynamic attributes per category (e.g., cars: brand/model/year/mileage).

### 3.3 Listings (Auction & Buy Now)

* `Listing`: Id, SellerId, CategoryId, Title, Slug, Description, Media\[], Location, Attributes (JSON),
  **Type** (Auction|BuyNow|Both), **Status** (Draft|PendingReview|Rejected|Approved|Active|Paused|Sold|Expired|Cancelled),
  **Auction**: StartAt, EndAt, StartPrice, ReservePrice?, BidIncrement, BuyNowPrice?, Views, WatchCount.
  **Moderation**: Notes, RejectionReason.
* `ListingMedia`: ListingId, Url, Type (Image|Video), SortOrder, IsCover.

### 3.4 Bids & Watchlists

* `Bid`: Id, ListingId, BidderId, Amount, PlacedAt, Status (Placed|Outbid|Winning|Retracted|Invalid).
* `Watchlist`: Id, UserId, ListingId, CreatedAt.

### 3.5 Orders & Payments (MVP placeholders)

* `Order`: Id, ListingId, BuyerId, SellerId, Price, Status (Pending|Paid|Cancelled|Failed|Refunded), CreatedAt.
  *Payments integration reserved for Phase 2; include CRUD stubs.*

### 3.6 Ratings & Reports (Phase 2)

* `Review`: Id, FromUserId, ToUserId, OrderId?, Score 1–5, Comment, Status (Pending|Published|Hidden).
* `Report`: Id, ReporterId, TargetType (Listing|User|Bid), TargetId, Reason, Status (Open|InReview|Resolved|Rejected).

### 3.7 CMS

* `Page`: Id, Slug, Title, Blocks (JSON), Seo (title, desc, og), Status (Draft|Published|Archived).
* `Menu`: Id, Key, Items (JSON).
* `MediaAsset`: Id, Url, Mime, Size, Folder, AltText, Tags.

---

## 4) Status Workflows (Key)

### Listing

* **Draft → PendingReview → (Approved|Rejected)** → Active
* **Active → (Paused|Sold|Expired|Cancelled)**
* Transitions guarded by role & business rules (time windows, reserve, bids present).

### Bid

* **Placed → Winning** (highest) or **Outbid** (when a higher bid arrives).
* **Winning → (Order Pending)** once auction ends.

### Page (CMS)

* **Draft ↔ Published ↔ Archived** (Editors/Admins only).

---

## 5) API Conventions

* **Base URL:** `/api/v1`
* **Auth:** Bearer tokens; role/scope-gated endpoints.
* **Pagination:** `X-Total-Count` header + `items` array.
* **Errors:** RFC 7807 Problem Details.

```json
// Paged response
{
  "items": [ /* ... */ ],
  "page": 1,
  "pageSize": 20,
  "total": 357
}
```

---

## 6) Endpoints (CRUD + Status)

> Below is the MVP **complete surface**. “Role/Scope” indicates who can call it. Request/response bodies omitted where trivial CRUD; focus on special fields & transitions.

### 6.1 Auth & Accounts

* `POST /auth/token` — OAuth flows (OpenIddict). *(public)*
* `GET /auth/userinfo` — OIDC user info. *(logged in)*
* `POST /auth/register` — email/password registration (Bidder/Seller). *(public)*
* `POST /auth/verify-email` — code; `POST /auth/resend-email`.
* `POST /auth/forgot-password`, `POST /auth/reset-password`.
* `GET /accounts/me` — profile; `PUT /accounts/me` — update profile. *(any user)*
* `GET /accounts/me/security` — 2FA status; `POST /accounts/me/security/2fa/enable|disable`.
* `GET /admin/users` (filter by role/status), `GET /admin/users/{id}`, `POST /admin/users` (invite), `PUT /admin/users/{id}`, `DELETE /admin/users/{id}` — *(Admin, scope `mazad.admin`)*
* `POST /admin/users/{id}/roles` — set roles.

### 6.2 Categories & Attributes (Admin + Public read)

* `GET /categories` (tree), `GET /categories/{id}` — *(public)*
* `POST /admin/categories`, `PUT /admin/categories/{id}`, `DELETE /admin/categories/{id}` — *(Admin)*
* `GET /categories/{id}/attributes` — schema (public)
* `POST /admin/categories/{id}/attributes` — define/patch attributes (Admin)

### 6.3 Vehicles Taxonomy (Admin + Public read)

* `GET /vehicles/brands`, `/brands/{id}/models`, `/models/{id}/trims` — *(public)*
* Admin CRUD for brands/models/trims:

  * `POST /admin/vehicles/brands` | `PUT /admin/vehicles/brands/{id}` | `DELETE ...`
  * `POST /admin/vehicles/brands/{brandId}/models`
  * `POST /admin/vehicles/models/{modelId}/trims`

### 6.4 Listings (Seller CRUD, Admin moderation, Public browse)

**Public/Bidder**

* `GET /listings` — search/browse: `q, categoryId, brand, model, yearMin, yearMax, condition, priceMin, priceMax, sort`.
* `GET /listings/{id}` — detail.
* `GET /listings/{id}/bids` — public top bids (limited).
* `POST /listings/{id}/watch` — add to watchlist *(Bidder)*; `DELETE /listings/{id}/watch`.

**Seller**

* `GET /seller/listings` — own items.
* `POST /seller/listings` — **create** (Draft).
* `PUT /seller/listings/{id}` — update allowed fields (if Draft|Rejected|PendingReview|Paused).
* `DELETE /seller/listings/{id}` — soft delete (if Draft|Rejected).
* **Status actions:**

  * `POST /seller/listings/{id}/submit` — Draft→PendingReview
  * `POST /seller/listings/{id}/pause` — Active→Paused
  * `POST /seller/listings/{id}/resume` — Paused→Active
  * `POST /seller/listings/{id}/cancel` — Active|PendingReview→Cancelled (guard: no bids, or admin override)
* **Media:**

  * `POST /seller/listings/{id}/media` (multipart) — upload
  * `DELETE /seller/listings/{id}/media/{mediaId}`

**Admin Moderation**

* `GET /admin/listings?status=PendingReview`
* `POST /admin/listings/{id}/approve` — PendingReview→Approved→Active (sets go-live)
* `POST /admin/listings/{id}/reject` — PendingReview→Rejected (body: reason)
* `POST /admin/listings/{id}/force-close` — Active→(Sold|Cancelled|Expired) w/ reason (audit)

### 6.5 Bids (Bidder)

* `GET /bids/my` — my bids
* `POST /listings/{id}/bids` — place bid `{amount}` (validates increment/reserve/time)
* `GET /bids/{id}` — bid detail
* `POST /bids/{id}/retract` — if policy allows (guard by time/status) *(optional)*

**Status logic (server-side):**

* New bid → compute **Winning/Outbid**; mark previous highest as Outbid; push notifications.
* At auction end → highest Winning → **Order Pending** (Phase 2 Orders).

### 6.6 Watchlists (Bidder)

* `GET /watchlists/my`
* `POST /watchlists` `{ listingId }`
* `DELETE /watchlists/{id}`

### 6.7 Orders & Payments (MVP stubs; full in Phase 2)

* `GET /orders/my` (Bidder/Seller filtered)
* `GET /orders/{id}`
* `POST /orders` (from Winning bid) — **server-initiated in auction close job**
* `PUT /orders/{id}/status` — Admin/Seller limited updates
* `POST /orders/{id}/pay` — placeholder (no gateway in MVP1)
* **Statuses:** Pending|Paid|Cancelled|Failed|Refunded

### 6.8 Reviews & Reports (Phase 2)

* `POST /reviews` `{toUserId, orderId, score, comment}`
* `GET /users/{id}/reviews`
* `POST /reports` `{targetType, targetId, reason}`
* Admin: `GET/PUT /admin/reviews/{id}`, `GET/PUT /admin/reports/{id}`

### 6.9 CMS

* **Pages**

  * `GET /cms/pages?status=Published&slug=...` *(public for website)*
  * `GET /cms/pages/{id}` *(auth for editors)*
  * `POST /cms/pages` (Draft), `PUT /cms/pages/{id}`, `DELETE /cms/pages/{id}` *(CMS\_Editor/Admin)*
  * `POST /cms/pages/{id}/publish`, `POST /cms/pages/{id}/archive` *(CMS\_Editor/Admin)*
* **Menus**

  * `GET /cms/menus/{key}` *(public)*
  * `POST/PUT/DELETE /cms/menus` *(CMS\_Editor/Admin)*
* **Media**

  * `POST /cms/media` (multipart), `GET /cms/media/{id}`, `DELETE /cms/media/{id}` *(CMS\_Editor/Admin)*

### 6.10 Admin – Governance

* **Users & Roles**: listed above.
* **Taxonomy Management**: categories/vehicles.
* **Moderation**: listings/reviews/reports.
* **Settings**: `GET/PUT /admin/settings` (JSON key/values: bids increment rules, time buffers, content policies).
* **Audit Log**: `GET /admin/audit` (paged) — actions with actor/time/entity.

---

## 7) Data Contracts (Selected)

### 7.1 Listing (Create/Update)

```json
{
  "title": "2019 Toyota Land Cruiser GXR",
  "categoryId": "cars",
  "type": "Auction",
  "description": "Well maintained...",
  "location": { "city": "Riyadh", "country": "SA" },
  "attributes": {
    "brand": "toyota",
    "model": "land-cruiser",
    "year": 2019,
    "trim": "gxr",
    "condition": "used",
    "mileage_km": 88000,
    "owners_count": 1,
    "accident_history": "none",
    "warranty_status": "expired"
  },
  "auction": { "startAt": "2025-10-01T12:00:00Z", "endAt": "2025-10-05T12:00:00Z", "startPrice": 100000, "reservePrice": 120000, "bidIncrement": 1000 },
  "buyNowPrice": null
}
```

### 7.2 Bid (Create)

```json
{ "amount": 121000 }
```

### 7.3 CMS Page

```json
{
  "slug": "home",
  "title": "Mazad — Welcome",
  "status": "Draft",
  "seo": { "title": "Mazad | Auctions", "description": "Buy & sell vehicles..." },
  "blocks": [
    { "type": "hero", "props": { "heading": "Find your next car", "cta": "/categories/vehicles" } },
    { "type": "grid", "props": { "items": [/* ... */] } }
  ]
}
```

---

## 8) EF Core (Code-First) Hints

* **BaseEntity**: `Id (Guid)`, `CreatedAt/By`, `UpdatedAt/By`, `IsDeleted`.
* **Owned types**: `Money`, `Location`.
* **Query Filters**: soft delete.
* **Indexes**: Listing(Status, EndAt), Bid(ListingId, Amount DESC).
* **Concurrency**: RowVersion for bids/listings.
* **Migrations**: `dotnet ef migrations add Init`, `dotnet ef database update`.

---

## 9) Security & Policies

* **Rate limiting**: anonymous browse/bid endpoints.
* **Seller cannot edit** once auction has bids (except media description fix; configurable).
* **Bid validation**: must be ≥ currentHigh + increment; reserve not disclosed.
* **Closing windows**: grace period, tie-breakers by timestamp.
* **CMS**: content sanitization (HTML), media MIME allowlist.
* **CORS**: allow admin/seller/bidder frontends.

---

## 10) OAuth2/OpenIddict Setup (summary)

* **Grants:** Authorization Code + PKCE (panels), Client Credentials (jobs), Refresh Tokens (panels).
* **Claims:** `sub, email, name, role, preferred_username, locale`.
* **Scopes → Policies mapping:**

  * `mazad.admin` → `[Authorize(Roles="Admin", Policy="Scope:mazad.admin")]`
  * `mazad.seller` → `[Authorize(Roles="Seller", Policy="Scope:mazad.seller")]`
  * `mazad.bidder` → `[Authorize(Roles="Bidder", Policy="Scope:mazad.bidder")]`
  * `mazad.cms` → `[Authorize(Roles="CMS_Editor,CMS_Admin", Policy="Scope:mazad.cms")]`

---

## 11) Example Controller Snippets (Minimal API style, abridged)

```csharp
// Program.cs (WebApi)
builder.Services
  .AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")))
  .AddIdentity<AppUser, AppRole>(/*...*/).AddEntityFrameworkStores<AppDbContext>()
  .AddOpenIddict()
    .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<AppDbContext>())
    .AddServer(o => {
       o.AllowAuthorizationCode().RequireProofKeyForCodeExchange();
       o.AllowRefreshToken();
       o.SetTokenEndpointUris("/auth/token");
       o.SetAuthorizationEndpointUris("/connect/authorize");
       o.RegisterScopes("mazad.api","mazad.admin","mazad.seller","mazad.bidder","mazad.cms","offline_access");
       o.UseAspNetCore().EnableTokenEndpointPassthrough().EnableAuthorizationEndpointPassthrough();
    })
    .AddValidation(o => o.UseLocalServer());
builder.Services.AddAuthorization(o => {
  o.AddPolicy("Scope:mazad.admin", p => p.RequireClaim("scope","mazad.admin"));
  // add others...
});
```

```csharp
// Listings endpoints (excerpt)
app.MapGroup("/api/v1/seller/listings").RequireAuthorization("Scope:mazad.seller").MapListingsSeller();

public static class SellerListingsEndpoints {
  public static RouteGroupBuilder MapListingsSeller(this RouteGroupBuilder g) {
    g.MapGet("/", async (IMediator m, ClaimsPrincipal user, int page=1,int pageSize=20) =>
       await m.Send(new GetMyListingsQuery(user.GetUserId(), page, pageSize)));
    g.MapPost("/", async (IMediator m, CreateListingCommand cmd, ClaimsPrincipal u) =>
       Results.Created($"/api/v1/seller/listings", await m.Send(cmd with { SellerId = u.GetUserId() })));
    g.MapPost("/{id:guid}/submit", async (IMediator m, Guid id, ClaimsPrincipal u) =>
       Results.Ok(await m.Send(new SubmitListingCommand(id, u.GetUserId()))));
    // pause/resume/cancel...
    return g;
  }
}
```

---

## 12) Seed & Bootstrapping

* **Default Admin** with `mazad.admin` + `mazad.cms`.
* **Sample Categories** + Vehicles (Toyota/Nissan/Hyundai/Kia/Ford/Lexus/BMW/Mercedes).
* **Demo Listings** with mixed statuses.
* **OpenAPI** (Swashbuckle) with OAuth2 security scheme for interactive testing.

---

## 13) Definition of Done (Backend)

* All entities & migrations created; database up on MSSQL.
* OAuth2/OIDC flows working; roles/scopes enforced.
* Every listed resource has **CRUD** + required **status actions**.
* Search/browse for listings with filters, pagination, sort.
* CMS endpoints power the public site (pages/menus/media).
* Moderation & audit trails functioning.
* Integration & unit tests for critical paths (bids, transitions).

---

## 14) Full Endpoint Index (Quick Reference)

### Auth/Accounts

* `POST /auth/token`, `GET /auth/userinfo`, `POST /auth/register`, `POST /auth/verify-email`, `POST /auth/forgot-password`, `POST /auth/reset-password`
* `GET /accounts/me`, `PUT /accounts/me`, `GET /accounts/me/security`, `POST /accounts/me/security/2fa/{toggle}`

### Users (Admin)

* `GET/POST/PUT/DELETE /admin/users`, `POST /admin/users/{id}/roles`

### Categories & Attributes

* `GET /categories`, `GET /categories/{id}`, `GET /categories/{id}/attributes`
* `POST/PUT/DELETE /admin/categories`, `POST /admin/categories/{id}/attributes`

### Vehicles Taxonomy

* `GET /vehicles/brands`, `GET /vehicles/brands/{id}/models`, `GET /vehicles/models/{id}/trims`
* `POST/PUT/DELETE /admin/vehicles/brands|models|trims`

### Listings

* Public/Bidder: `GET /listings`, `GET /listings/{id}`, `GET /listings/{id}/bids`, `POST/DELETE /listings/{id}/watch`
* Seller: `GET /seller/listings`, `POST /seller/listings`, `PUT /seller/listings/{id}`, `DELETE /seller/listings/{id}`,
  `POST /seller/listings/{id}/submit|pause|resume|cancel`,
  `POST /seller/listings/{id}/media`, `DELETE /seller/listings/{id}/media/{mediaId}`
* Admin: `GET /admin/listings?status=...`, `POST /admin/listings/{id}/approve|reject|force-close`

### Bids

* `POST /listings/{id}/bids`, `GET /bids/{id}`, `GET /bids/my`, `POST /bids/{id}/retract` (if enabled)

### Watchlists

* `GET /watchlists/my`, `POST /watchlists`, `DELETE /watchlists/{id}`

### Orders (MVP stubs)

* `GET /orders/my`, `GET /orders/{id}`, `POST /orders`, `PUT /orders/{id}/status`, `POST /orders/{id}/pay`

### Reviews & Reports (P2)

* `POST /reviews`, `GET /users/{id}/reviews`, `POST /reports`, `GET/PUT /admin/reviews/{id}`, `GET/PUT /admin/reports/{id}`

### CMS

* Pages: `GET /cms/pages`, `GET /cms/pages/{id}`, `POST/PUT/DELETE /cms/pages`, `POST /cms/pages/{id}/publish|archive`
* Menus: `GET /cms/menus/{key}`, `POST/PUT/DELETE /cms/menus`
* Media: `POST /cms/media`, `GET /cms/media/{id}`, `DELETE /cms/media/{id}`

### Admin – Settings & Audit

* `GET/PUT /admin/settings`, `GET /admin/audit`

---

## 15) Next Steps (what I’d generate next if you want code)

1. **Solution skeleton** with the four projects and wired DI/auth.
2. **EF Core models + Migrations** and seeders.
3. **Minimal APIs/Controllers** implementing the endpoint index.
4. **Swagger** with OAuth2 and scopes, example requests.
5. **Sample Postman/Thunder tests** for CRUD + transitions.

If you want, I can spin up the actual **.NET 8 solution scaffold** with folders, example entities, and a few fully implemented endpoints (Listings CRUD + Seller submit/ pause/ resume + Admin approve/reject) in a follow-up.
