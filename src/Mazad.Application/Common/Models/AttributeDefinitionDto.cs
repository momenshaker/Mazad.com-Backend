namespace Mazad.Application.Common.Models;

public record AttributeDefinitionDto(
    Guid Id,
    Guid CategoryId,
    string Key,
    string DisplayName,
    string DataType,
    string? OptionsJson);
