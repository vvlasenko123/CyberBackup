using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Domain.Calendar;

namespace Application.Abstractions.UseCases.Calendar;

/// <inheritdoc />
public sealed class GetCalendarEventsUseCaseManager : IGetCalendarEventsUseCaseManager
{
    private readonly ICalendarEventService _calendarEventService;

    public GetCalendarEventsUseCaseManager(ICalendarEventService calendarEventService)
    {
        _calendarEventService = calendarEventService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> Execute(CancellationToken cancellationToken)
    {
        return await _calendarEventService.GetAll(cancellationToken);
    }
}