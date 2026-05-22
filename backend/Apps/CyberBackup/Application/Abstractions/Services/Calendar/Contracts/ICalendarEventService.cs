using Application.DTO.Calendar;
using Domain.Calendar;

namespace Application.Abstractions.Services.Calendar.Contracts;

/// <summary>
/// Сервис событий календаря
/// </summary>
public interface ICalendarEventService
{
    /// <summary>
    /// Создать событие календаря
    /// </summary>
    Task<CalendarEventModel> Create(CalendarEventDto request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить события пользователя
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetByUserId(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить все события календаря
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> GetAll(CancellationToken cancellationToken);
}