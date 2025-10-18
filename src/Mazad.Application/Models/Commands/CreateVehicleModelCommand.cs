using System;
using System.Text.RegularExpressions;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Taxonomy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Models.Commands;

public record CreateVehicleModelCommand(Guid BrandId, string Name, string? Slug) : IRequest<VehicleModelDto>;

public class CreateVehicleModelCommandHandler : IRequestHandler<CreateVehicleModelCommand, VehicleModelDto>
{
    private readonly IMazadDbContext _context;

    public CreateVehicleModelCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleModelDto> Handle(CreateVehicleModelCommand request, CancellationToken cancellationToken)
    {
        var brandExists = await _context.VehicleBrands
            .AnyAsync(b => b.Id == request.BrandId, cancellationToken);

        if (!brandExists)
        {
            throw new NotFoundException("VehicleBrand", request.BrandId);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        var exists = await _context.VehicleModels
            .AnyAsync(m => m.BrandId == request.BrandId && m.Slug == slug, cancellationToken);

        if (exists)
        {
            throw new ConflictException("VehicleModel", slug);
        }

        var model = new VehicleModel
        {
            Id = Guid.NewGuid(),
            BrandId = request.BrandId,
            Name = request.Name.Trim(),
            Slug = slug,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.VehicleModels.AddAsync(model, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleModelDto(model.Id, model.BrandId, model.Name, model.Slug);
    }

    private static string GenerateSlug(string value)
    {
        var slug = value.ToLowerInvariant();
        slug = Regex.Replace(slug, "[^a-z0-9]+", "-");
        return slug.Trim('-');
    }
}
