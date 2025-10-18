using System;
using System.Text.RegularExpressions;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Taxonomy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Brands.Commands;

public record CreateVehicleBrandCommand(string Name, string? Slug) : IRequest<VehicleBrandDto>;

public class CreateVehicleBrandCommandHandler : IRequestHandler<CreateVehicleBrandCommand, VehicleBrandDto>
{
    private readonly IMazadDbContext _context;

    public CreateVehicleBrandCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleBrandDto> Handle(CreateVehicleBrandCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        var exists = await _context.VehicleBrands
            .AnyAsync(b => b.Slug == slug, cancellationToken);

        if (exists)
        {
            throw new ConflictException("VehicleBrand", slug);
        }

        var brand = new VehicleBrand
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.VehicleBrands.AddAsync(brand, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleBrandDto(brand.Id, brand.Name, brand.Slug);
    }

    private static string GenerateSlug(string value)
    {
        var slug = value.ToLowerInvariant();
        slug = Regex.Replace(slug, "[^a-z0-9]+", "-");
        return slug.Trim('-');
    }
}
