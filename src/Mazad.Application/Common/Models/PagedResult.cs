namespace Mazad.Application.Common.Models;

public record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
