using Domain.Calendar;

namespace Application.Abstractions.UseCases.Calendar.Contracts;

/// <summary>
/// Менеджер получения событий календаря
/// </summary>
public interface IGetCalendarEventsUseCaseManager
{
    /// <summary>
    /// Получить события календаря текущего пользователя
    /// </summary>
    Task<IReadOnlyCollection<CalendarEventModel>> Execute(CancellationToken cancellationToken);
}