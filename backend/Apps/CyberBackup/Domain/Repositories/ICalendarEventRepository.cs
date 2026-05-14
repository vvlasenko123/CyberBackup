using Domain.Calendar;

namespace Domain.Repositories;

/// <summary>
/// Репозиторий событий календаря
/// </summary>
public interface ICalendarEventRepository
{
    /// <summary>
    /// Создать событие календаря
    /// </summary>
    Task CreateAsync(CalendarEventModel calendarEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Получить события пользователя
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить события для уведомления
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetForNotificationAsync(DateTimeOffset nowUtc, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Отметить событие как уведомленное
    /// </summary>
    Task SetNotifiedAsync(Guid id, DateTimeOffset notifiedAtUtc, CancellationToken cancellationToken);
}