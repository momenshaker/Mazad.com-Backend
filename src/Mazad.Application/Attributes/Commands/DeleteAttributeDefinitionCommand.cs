using System;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Attributes.Commands;

public record DeleteAttributeDefinitionCommand(Guid Id) : IRequest;

public class DeleteAttributeDefinitionCommandHandler : IRequestHandler<DeleteAttributeDefinitionCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public DeleteAttributeDefinitionCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var attribute = await _context.AttributeDefinitions
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attribute is null)
        {
            throw new NotFoundException("AttributeDefinition", request.Id);
        }

        _context.AttributeDefinitions.Remove(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
