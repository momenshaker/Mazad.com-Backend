using Mazad.Application.DependencyInjection;
using Mazad.Infrastructure.DependencyInjection;
using Mazad.WebApi.Endpoints.Categories;
using Mazad.WebApi.Endpoints.Listings;
using Microsoft.AspNetCore.Authentication.Cookies;

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
app.MapPublicCategoriesEndpoints();
app.MapAdminCategoriesEndpoints();

app.Run();
