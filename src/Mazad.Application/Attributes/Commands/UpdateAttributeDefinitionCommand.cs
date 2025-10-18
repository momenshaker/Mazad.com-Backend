using System;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Attributes.Commands;

public record UpdateAttributeDefinitionCommand(Guid Id, string DisplayName, string DataType, string? OptionsJson) : IRequest<AttributeDefinitionDto>;

public class UpdateAttributeDefinitionCommandHandler : IRequestHandler<UpdateAttributeDefinitionCommand, AttributeDefinitionDto>
{
    private readonly IMazadDbContext _context;

    public UpdateAttributeDefinitionCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<AttributeDefinitionDto> Handle(UpdateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var attribute = await _context.AttributeDefinitions
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attribute is null)
        {
            throw new NotFoundException("AttributeDefinition", request.Id);
        }

        attribute.DisplayName = request.DisplayName;
        attribute.DataType = request.DataType;
        attribute.OptionsJson = request.OptionsJson;
        attribute.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new AttributeDefinitionDto(attribute.Id, attribute.CategoryId, attribute.Key, attribute.DisplayName, attribute.DataType, attribute.OptionsJson);
    }
}
