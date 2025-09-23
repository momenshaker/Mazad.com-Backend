using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Categories.Commands;

public record CreateCategoryCommand(Guid? ParentId, string Name, string? Slug, string? AttributesSchema) : IRequest<CategoryDto>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IMazadDbContext _context;

    public CreateCategoryCommandHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException("Category", request.ParentId.Value);
            }
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug!.ToLowerInvariant();

        var slugExists = await _context.Categories.AnyAsync(c => c.Slug == slug, cancellationToken);
        if (slugExists)
        {
            throw new BusinessRuleException($"A category with slug '{slug}' already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            Name = request.Name,
            Slug = slug,
            AttributesSchema = request.AttributesSchema
        };

        await _context.Categories.AddAsync(category, cancellationToken);
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
