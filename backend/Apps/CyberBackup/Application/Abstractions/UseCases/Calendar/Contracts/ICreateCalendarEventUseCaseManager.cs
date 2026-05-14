using Application.DTO.Calendar;
using Domain.Calendar;

namespace Application.Abstractions.UseCases.Calendar.Contracts;

/// <summary>
/// Менеджер создания события календаря
/// </summary>
public interface ICreateCalendarEventUseCaseManager
{
    /// <summary>
    /// Создать событие календаря
    /// </summary>
    Task<CalendarEventModel> Execute(CalendarEventDto request, CancellationToken cancellationToken);
}