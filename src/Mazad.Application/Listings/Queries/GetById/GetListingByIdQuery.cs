using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Queries.GetById;

public record GetListingByIdQuery(Guid Id) : IRequest<ListingDto>;

public class GetListingByIdQueryHandler : IRequestHandler<GetListingByIdQuery, ListingDto>
{
    private readonly IMazadDbContext _context;

    public GetListingByIdQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .AsNoTracking()
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.Id);
        }

        return listing.ToDto();
    }
}
