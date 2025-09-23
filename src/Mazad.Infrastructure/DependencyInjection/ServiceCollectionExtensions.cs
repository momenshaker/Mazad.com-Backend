using Mazad.Application.Abstractions.Persistence;
using Mazad.Infrastructure.Identity;
using Mazad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mazad.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:ConnectionString"]
            ?? "Server=(localdb)\\mssqllocaldb;Database=MazadDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<MazadDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(MazadDbContext).Assembly.FullName);
            });
            options.UseOpenIddict();
        });

        services.AddScoped<IMazadDbContext>(sp => sp.GetRequiredService<MazadDbContext>());

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<MazadDbContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore()
                    .UseDbContext<MazadDbContext>();
            })
            .AddServer(builder =>
            {
                builder.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                builder.RegisterScopes(
                    "openid",
                    "profile",
                    "email",
                    "roles",
                    "mazad.api",
                    "mazad.admin",
                    "mazad.seller",
                    "mazad.bidder",
                    "mazad.cms",
                    "offline_access");

                builder.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddIdentityCookies();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Scope:mazad.admin", policy => policy.RequireClaim("scope", "mazad.admin"));
            options.AddPolicy("Scope:mazad.seller", policy => policy.RequireClaim("scope", "mazad.seller"));
            options.AddPolicy("Scope:mazad.bidder", policy => policy.RequireClaim("scope", "mazad.bidder"));
            options.AddPolicy("Scope:mazad.cms", policy => policy.RequireClaim("scope", "mazad.cms"));
        });

        return services;
    }
}
