using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Brands.Commands;

public record DeleteVehicleBrandCommand(Guid Id) : IRequest<Unit>;

public class DeleteVehicleBrandCommandHandler : IRequestHandler<DeleteVehicleBrandCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public DeleteVehicleBrandCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteVehicleBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.VehicleBrands
            .Include(b => b.Models)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException("VehicleBrand", request.Id);
        }

        if (brand.Models.Any())
        {
            throw new BusinessRuleException("Cannot delete a brand that still has models.");
        }

        _context.VehicleBrands.Remove(brand);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
