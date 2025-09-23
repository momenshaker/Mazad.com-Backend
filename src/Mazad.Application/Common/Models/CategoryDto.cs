namespace Mazad.Application.Common.Models;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public string? AttributesSchema { get; init; }
    public IReadOnlyCollection<CategoryDto> Children { get; init; } = Array.Empty<CategoryDto>();
}
