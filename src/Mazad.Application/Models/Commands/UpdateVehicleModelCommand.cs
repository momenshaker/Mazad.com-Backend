using System;
using System.Text.RegularExpressions;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Models.Commands;

public record UpdateVehicleModelCommand(Guid Id, string Name, string? Slug) : IRequest<VehicleModelDto>;

public class UpdateVehicleModelCommandHandler : IRequestHandler<UpdateVehicleModelCommand, VehicleModelDto>
{
    private readonly IMazadDbContext _context;

    public UpdateVehicleModelCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleModelDto> Handle(UpdateVehicleModelCommand request, CancellationToken cancellationToken)
    {
        var model = await _context.VehicleModels
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (model is null)
        {
            throw new NotFoundException("VehicleModel", request.Id);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? model.Slug
            : request.Slug.Trim().ToLowerInvariant();

        if (!string.Equals(slug, model.Slug, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.VehicleModels
                .AnyAsync(m => m.BrandId == model.BrandId && m.Id != model.Id && m.Slug == slug, cancellationToken);

            if (exists)
            {
                throw new ConflictException("VehicleModel", slug);
            }

            model.Slug = slug;
        }

        model.Name = request.Name.Trim();
        model.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleModelDto(model.Id, model.BrandId, model.Name, model.Slug);
    }
}
