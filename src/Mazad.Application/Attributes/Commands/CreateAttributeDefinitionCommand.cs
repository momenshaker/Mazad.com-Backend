using System;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Attributes.Commands;

public record CreateAttributeDefinitionCommand(Guid CategoryId, string Key, string DisplayName, string DataType, string? OptionsJson) : IRequest<AttributeDefinitionDto>;

public class CreateAttributeDefinitionCommandHandler : IRequestHandler<CreateAttributeDefinitionCommand, AttributeDefinitionDto>
{
    private readonly IMazadDbContext _context;

    public CreateAttributeDefinitionCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<AttributeDefinitionDto> Handle(CreateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        var exists = await _context.AttributeDefinitions
            .AnyAsync(a => a.CategoryId == request.CategoryId && a.Key == request.Key, cancellationToken);

        if (exists)
        {
            throw new ConflictException("AttributeDefinition", request.Key);
        }

        var attribute = new AttributeDefinition
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Key = request.Key,
            DisplayName = request.DisplayName,
            DataType = request.DataType,
            OptionsJson = request.OptionsJson,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.AttributeDefinitions.AddAsync(attribute, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AttributeDefinitionDto(attribute.Id, attribute.CategoryId, attribute.Key, attribute.DisplayName, attribute.DataType, attribute.OptionsJson);
    }
}
