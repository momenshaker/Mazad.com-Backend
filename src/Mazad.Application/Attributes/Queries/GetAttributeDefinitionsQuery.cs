using System;
using System.Collections.Generic;
using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Attributes.Queries;

public record GetAttributeDefinitionsQuery(Guid? CategoryId = null) : IRequest<IReadOnlyCollection<AttributeDefinitionDto>>;

public class GetAttributeDefinitionsQueryHandler : IRequestHandler<GetAttributeDefinitionsQuery, IReadOnlyCollection<AttributeDefinitionDto>>
{
    private readonly IMazadDbContext _context;

    public GetAttributeDefinitionsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<AttributeDefinitionDto>> Handle(GetAttributeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AttributeDefinitions
            .AsNoTracking()
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        var attributes = await query
            .OrderBy(a => a.DisplayName)
            .Select(a => new AttributeDefinitionDto(a.Id, a.CategoryId, a.Key, a.DisplayName, a.DataType, a.OptionsJson))
            .ToListAsync(cancellationToken);

        return attributes;
    }
}
