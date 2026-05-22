using Domain.Calendar.Enums;

namespace Api.Controllers.Models.Response;

/// <summary>
/// Ответ с событием календаря
/// </summary>
public sealed record CalendarEventResponse(
    Guid Id,
    Guid UserId,
    string Title,
    CalendarEventType EventType,
    CalendarEventStatus Status,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    DateTimeOffset? NotifyAtUtc,
    DateTimeOffset? NotifiedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);