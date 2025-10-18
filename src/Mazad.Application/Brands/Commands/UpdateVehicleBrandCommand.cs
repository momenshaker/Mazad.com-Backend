using System;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Brands.Commands;

public record UpdateVehicleBrandCommand(Guid Id, string Name, string? Slug) : IRequest<VehicleBrandDto>;

public class UpdateVehicleBrandCommandHandler : IRequestHandler<UpdateVehicleBrandCommand, VehicleBrandDto>
{
    private readonly IMazadDbContext _context;

    public UpdateVehicleBrandCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleBrandDto> Handle(UpdateVehicleBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.VehicleBrands
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException("VehicleBrand", request.Id);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? brand.Slug
            : request.Slug.Trim().ToLowerInvariant();

        if (!string.Equals(slug, brand.Slug, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.VehicleBrands
                .AnyAsync(b => b.Id != brand.Id && b.Slug == slug, cancellationToken);

            if (exists)
            {
                throw new ConflictException("VehicleBrand", slug);
            }

            brand.Slug = slug;
        }

        brand.Name = request.Name.Trim();
        brand.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleBrandDto(brand.Id, brand.Name, brand.Slug);
    }
}
