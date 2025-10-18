namespace Mazad.Application.Common.Models;

public record ListingHistoryDto(Guid ListingId, IReadOnlyCollection<ListingHistoryEventDto> Events);

public record ListingHistoryEventDto(string Type, DateTimeOffset OccurredAt, string Description, decimal? Amount = null, Guid? ActorId = null);
