using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Listings.Commands.Moderation;

public record UpdateListingStatusAdminCommand(Guid ListingId, ListingStatus TargetStatus, Guid AdminId, string? Notes = null) : IRequest<ListingDto>;

public class UpdateListingStatusAdminCommandHandler : IRequestHandler<UpdateListingStatusAdminCommand, ListingDto>
{
    private readonly IMazadDbContext _context;

    public UpdateListingStatusAdminCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<ListingDto> Handle(UpdateListingStatusAdminCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        if (request.TargetStatus is not (ListingStatus.Cancelled or ListingStatus.Sold or ListingStatus.Expired))
        {
            throw new BusinessRuleException("Only finalization statuses can be set via this endpoint.");
        }

        listing.Status = request.TargetStatus;
        listing.ModerationNotes = request.Notes;
        listing.UpdatedAt = DateTimeOffset.UtcNow;
        listing.UpdatedById = request.AdminId;

        _context.AuditLogs.Add(new Mazad.Domain.Entities.Admin.AuditLogEntry
        {
            Id = Guid.NewGuid(),
            Actor = request.AdminId.ToString(),
            Area = "Auctions",
            Action = request.TargetStatus.ToString(),
            Description = $"Listing {listing.Id} status set to {request.TargetStatus}",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = request.AdminId
        });

        await _context.SaveChangesAsync(cancellationToken);
        return listing.ToDto();
    }
}
