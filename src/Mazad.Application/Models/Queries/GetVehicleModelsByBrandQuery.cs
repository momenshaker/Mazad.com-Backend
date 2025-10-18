using System;
using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Models.Queries;

public record GetVehicleModelsByBrandQuery(Guid BrandId) : IRequest<IReadOnlyCollection<VehicleModelDto>>;

public class GetVehicleModelsByBrandQueryHandler : IRequestHandler<GetVehicleModelsByBrandQuery, IReadOnlyCollection<VehicleModelDto>>
{
    private readonly IMazadDbContext _context;

    public GetVehicleModelsByBrandQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<VehicleModelDto>> Handle(GetVehicleModelsByBrandQuery request, CancellationToken cancellationToken)
    {
        var models = await _context.VehicleModels
            .AsNoTracking()
            .Where(m => m.BrandId == request.BrandId)
            .OrderBy(m => m.Name)
            .Select(m => new VehicleModelDto(m.Id, m.BrandId, m.Name, m.Slug))
            .ToListAsync(cancellationToken);

        return models;
    }
}
