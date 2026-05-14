using Infrastructure.Core.DDD.Aggregate;

namespace Domain.Calendar;

/// <summary>
/// Модель уведомления
/// </summary>
public sealed class NotificationModel : AggregateRoot<Guid>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Идентификатор события календаря
    /// </summary>
    public Guid? CalendarEventId { get; }

    /// <summary>
    /// Заголовок
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Текст
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Прочитано ли уведомление
    /// </summary>
    public bool IsRead { get; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    public NotificationModel() : base(Guid.Empty)
    {
        
    }
    
    public NotificationModel(
        Guid id,
        Guid userId,
        Guid? calendarEventId,
        string title,
        string message,
        bool isRead,
        DateTimeOffset createdAtUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        CalendarEventId = calendarEventId;
        Title = title;
        Message = message;
        IsRead = isRead;
        CreatedAtUtc = createdAtUtc;
    }
}