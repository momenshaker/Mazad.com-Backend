using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Categories.Queries;

public record GetCategoriesTreeQuery : IRequest<IReadOnlyCollection<CategoryDto>>;

public class GetCategoriesTreeQueryHandler : IRequestHandler<GetCategoriesTreeQuery, IReadOnlyCollection<CategoryDto>>
{
    private readonly IMazadDbContext _context;

    public GetCategoriesTreeQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> Handle(GetCategoriesTreeQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Children)
            .ToListAsync(cancellationToken);
        foreach (var category in categories)
        {
            category.Children = categories.Where(c => c.ParentId == category.Id).ToList();
        }

        var roots = categories.Where(c => c.ParentId == null).ToList();
        return roots.Select(c => c.ToDto()).ToArray();
    }
}
