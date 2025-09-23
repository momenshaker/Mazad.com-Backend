using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Listings;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Create;

public record CreateListingCommand : IRequest<ListingDto>
{
    public Guid SellerId { get; init; }
    public Guid CategoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? Attributes { get; init; }
    public ListingType Type { get; init; } = ListingType.Auction;
    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }
    public decimal? StartPrice { get; init; }
    public decimal? ReservePrice { get; init; }
    public decimal? BidIncrement { get; init; }
    public decimal? BuyNowPrice { get; init; }
    public IReadOnlyCollection<CreateListingMediaRequest> Media { get; init; } = Array.Empty<CreateListingMediaRequest>();
}

public record CreateListingMediaRequest
{
    public string Url { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsCover { get; init; }
    public int? SortOrder { get; init; }
}

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public CreateListingCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        var now = DateTimeOffset.UtcNow;

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = request.SellerId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Location = request.Location,
            Attributes = request.Attributes,
            Type = request.Type,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            StartPrice = request.StartPrice,
            ReservePrice = request.ReservePrice,
            BidIncrement = request.BidIncrement,
            BuyNowPrice = request.BuyNowPrice,
            CreatedAt = now,
            CreatedById = request.SellerId,
            Status = ListingStatus.Draft
        };

        var media = request.Media.Select((m, index) => new ListingMedia
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            Url = m.Url,
            Type = m.Type,
            IsCover = m.IsCover,
            SortOrder = m.SortOrder ?? index,
            CreatedAt = now,
            CreatedById = request.SellerId
        }).ToList();

        if (media.Count > 0)
        {
            listing.Media = media;
        }

        await _context.Listings.AddAsync(listing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }

    private static string GenerateSlug(string value)
    {
        var slug = new string(value.ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-').ToArray());
        slug = slug.Replace(' ', '-');
        slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-");
        return slug.Trim('-');
    }
}
