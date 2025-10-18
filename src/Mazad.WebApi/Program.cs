using Mazad.Application.DependencyInjection;
using Mazad.Infrastructure.DependencyInjection;
using Mazad.WebApi.Endpoints.Admin;
using Mazad.WebApi.Endpoints.Alerts;
using Mazad.WebApi.Endpoints.Auctions;
using Mazad.WebApi.Endpoints.Auth;
using Mazad.WebApi.Endpoints.Bids;
using Mazad.WebApi.Endpoints.Categories;
using Mazad.WebApi.Endpoints.Cms;
using Mazad.WebApi.Endpoints.Attributes;
using Mazad.WebApi.Endpoints.Brands;
using Mazad.WebApi.Endpoints.Listings;
using Mazad.WebApi.Endpoints.Notifications;
using Mazad.WebApi.Endpoints.Orders;
using Mazad.WebApi.Endpoints.Payments;
using Mazad.WebApi.Endpoints.Reviews;
using Mazad.WebApi.Endpoints.Search;
using Mazad.WebApi.Endpoints.Sellers;
using Mazad.WebApi.Endpoints.Shipping;
using Mazad.WebApi.Endpoints.Users;
using Mazad.WebApi.Endpoints.Watchlists;
using Mazad.WebApi.Endpoints.Disputes;
using Mazad.WebApi.Endpoints.Fees;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Mazad.com API", Version = "v1" });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mazad.com API v1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPublicListingsEndpoints();
app.MapSellerListingsEndpoints();
app.MapAdminListingsEndpoints();
app.MapListingManagementEndpoints();
app.MapPublicCategoriesEndpoints();
app.MapAdminCategoriesEndpoints();
app.MapBrandEndpoints();
app.MapAttributeEndpoints();
app.MapAuthEndpoints();
app.MapBidEndpoints();
app.MapWatchlistEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapAuctionEndpoints();
app.MapAlertEndpoints();
app.MapSellerEndpoints();
app.MapPaymentEndpoints();
app.MapFeeEndpoints();
app.MapOrderEndpoints();
app.MapShippingEndpoints();
app.MapDisputeEndpoints();
app.MapReviewEndpoints();
app.MapNotificationEndpoints();
app.MapSearchEndpoints();
app.MapCmsEndpoints();
app.MapAdminEndpoints();

app.Run();
