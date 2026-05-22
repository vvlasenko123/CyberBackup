using Domain.Calendar.Enums;
using Infrastructure.Core.DDD.Aggregate;

namespace Domain.Calendar;

/// <summary>
/// Модель события календаря
/// </summary>
public sealed class CalendarEventModel : AggregateRoot<Guid>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Тип события
    /// </summary>
    public CalendarEventType EventType { get; init; }

    /// <summary>
    /// Статус события
    /// </summary>
    public CalendarEventStatus Status { get; init; }

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

    /// <summary>
    /// Дата и время отправки уведомления
    /// </summary>
    public DateTimeOffset? NotifiedAtUtc { get; init; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Дата изменения
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    public CalendarEventModel() : base(Guid.Empty)
    {
    }

    public CalendarEventModel(
        Guid id,
        Guid userId,
        string title,
        CalendarEventType eventType,
        CalendarEventStatus status,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        DateTimeOffset? notifyAtUtc,
        DateTimeOffset? notifiedAtUtc,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        Title = title;
        EventType = eventType;
        Status = status;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        NotifyAtUtc = notifyAtUtc;
        NotifiedAtUtc = notifiedAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }
}