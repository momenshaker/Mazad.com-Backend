using System.Linq;
using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Catalog;

namespace Mazad.Application.Common.Mappings;

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentId = category.ParentId,
            AttributesSchema = category.AttributesSchema,
            Children = category.Children.Select(ToDto).ToArray()
        };
    }
}
