using System;
using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Models.Commands;

public record DeleteVehicleModelCommand(Guid Id) : IRequest;

public class DeleteVehicleModelCommandHandler : IRequestHandler<DeleteVehicleModelCommand>
{
    private readonly IMazadDbContext _context;

    public DeleteVehicleModelCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteVehicleModelCommand request, CancellationToken cancellationToken)
    {
        var model = await _context.VehicleModels
            .Include(m => m.Trims)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (model is null)
        {
            throw new NotFoundException("VehicleModel", request.Id);
        }

        if (model.Trims.Any())
        {
            throw new BusinessRuleException("Cannot delete a model that still has trims configured.");
        }

        _context.VehicleModels.Remove(model);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
