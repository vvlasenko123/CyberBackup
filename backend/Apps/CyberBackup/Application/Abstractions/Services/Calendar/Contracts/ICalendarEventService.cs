using Application.DTO.Calendar;
using Domain.Calendar;
using Domain.User.Enums;

namespace Application.Abstractions.Services.Calendar.Contracts;

/// <summary>
/// Сервис событий календаря
/// </summary>
public interface ICalendarEventService
{
    /// <summary>
    /// Создать событие календаря и разослать уведомления
    /// </summary>
    Task<CalendarEventModel> Create(CalendarEventDto request, Guid userId, UserRole creatorRole, CancellationToken cancellationToken);

    /// <summary>
    /// Получить события пользователя
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetByUserId(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все события календаря
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetAll(CancellationToken cancellationToken);

    /// <summary>
    /// Получить события видимые студенту
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetStudentEvents(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить события преподавателя: свои + админские
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetTeacherEvents(Guid teacherId, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить событие календаря. Если isAdmin = true — удаляет без проверки владельца.
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
}