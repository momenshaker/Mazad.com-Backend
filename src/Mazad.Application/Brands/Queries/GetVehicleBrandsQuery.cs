using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Brands.Queries;

public record GetVehicleBrandsQuery(string? Search = null) : IRequest<IReadOnlyCollection<VehicleBrandDto>>;

public class GetVehicleBrandsQueryHandler : IRequestHandler<GetVehicleBrandsQuery, IReadOnlyCollection<VehicleBrandDto>>
{
    private readonly IMazadDbContext _context;

    public GetVehicleBrandsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<VehicleBrandDto>> Handle(GetVehicleBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VehicleBrands
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(b => EF.Functions.Like(b.Name, $"%{term}%"));
        }

        var brands = await query
            .OrderBy(b => b.Name)
            .Select(b => new VehicleBrandDto(b.Id, b.Name, b.Slug))
            .ToListAsync(cancellationToken);

        return brands;
    }
}
