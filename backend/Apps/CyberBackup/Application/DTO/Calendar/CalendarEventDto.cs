using Domain.Calendar.Enums;

namespace Application.DTO.Calendar;

/// <summary>
/// DTO события календаря
/// </summary>
public sealed record CalendarEventDto
{
    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Тип события
    /// </summary>
    public CalendarEventType EventType { get; init; }

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    public DateTimeOffset StartsAtUtc { get; init; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public DateTimeOffset EndsAtUtc { get; init; }

    /// <summary>
    /// Дата и время уведомления
    /// </summary>
    public DateTimeOffset? NotifyAtUtc { get; init; }
}