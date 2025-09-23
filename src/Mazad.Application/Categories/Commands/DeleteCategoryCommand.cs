using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IMazadDbContext _context;

    public DeleteCategoryCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.Children)
            .Include(c => c.Listings)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        if (category.Children.Any())
        {
            throw new BusinessRuleException("Cannot delete a category that has child categories.");
        }

        if (category.Listings.Any())
        {
            throw new BusinessRuleException("Cannot delete a category that has listings assigned.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
