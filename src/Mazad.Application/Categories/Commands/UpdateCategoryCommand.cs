using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, string Name, string? Slug, string? AttributesSchema) : IRequest<CategoryDto>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IMazadDbContext _context;

    public UpdateCategoryCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug!.ToLowerInvariant();

        var slugConflict = await _context.Categories
            .AnyAsync(c => c.Id != request.Id && c.Slug == slug, cancellationToken);

        if (slugConflict)
        {
            throw new BusinessRuleException($"A category with slug '{slug}' already exists.");
        }

        category.Name = request.Name;
        category.Slug = slug;
        category.AttributesSchema = request.AttributesSchema;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    private static string GenerateSlug(string value)
    {
        var slug = new string(value.ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-').ToArray());
        slug = slug.Replace(' ', '-');
        slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-");
        return slug.Trim('-');
    }
}
