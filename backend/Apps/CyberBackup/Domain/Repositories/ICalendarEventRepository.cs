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
    /// Получить все события календаря
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить события для уведомления
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetForNotificationAsync(DateTimeOffset nowUtc, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Отметить событие как уведомленное
    /// </summary>
    Task SetNotifiedAsync(Guid id, DateTimeOffset notifiedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить событие календаря (только своё)
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить любое событие календаря (для администратора)
    /// </summary>
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить события видимые студенту: созданные его преподами + созданные администраторами
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetStudentEventsAsync(Guid studentId, CancellationToken cancellationToken);
}