using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Categories.Queries;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IMazadDbContext _context;

    public GetCategoryByIdQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        category.Children = await _context.Categories
            .Where(c => c.ParentId == category.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return category.ToDto();
    }
}
