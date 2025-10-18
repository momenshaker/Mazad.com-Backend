using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Application.Common.Mappings;
using Mazad.Domain.Entities.Listings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Media;

public record AddListingImagesCommand(Guid ListingId, Guid ActorId, bool IsAdmin, IReadOnlyCollection<AddListingImageRequest> Images) : IRequest<ListingDto>;

public record AddListingImageRequest(string Url, string Type, bool IsCover, int? SortOrder);

public class AddListingImagesCommandHandler : IRequestHandler<AddListingImagesCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public AddListingImagesCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(AddListingImagesCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (!request.IsAdmin && listing.SellerId != request.ActorId)
        {
            throw new BusinessRuleException("You do not have permission to modify this listing.");
        }

        var now = DateTimeOffset.UtcNow;
        var nextSortOrder = listing.Media.Count > 0 ? listing.Media.Max(m => m.SortOrder) + 1 : 0;

        foreach (var image in request.Images)
        {
            var media = new ListingMedia
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Url = image.Url,
                Type = image.Type,
                IsCover = image.IsCover,
                SortOrder = image.SortOrder ?? nextSortOrder++,
                CreatedAt = now,
                CreatedById = request.ActorId
            };

            listing.Media.Add(media);
        }

        listing.UpdatedAt = now;
        listing.UpdatedById = request.ActorId;

        await _context.SaveChangesAsync(cancellationToken);

        return listing.ToDto();
    }
}
