using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Domain.Calendar;

namespace Application.Abstractions.UseCases.Calendar;

/// <inheritdoc />
public sealed class GetCalendarEventsUseCaseManager : IGetCalendarEventsUseCaseManager
{
    private readonly ICalendarEventService _calendarEventService;
    private readonly IJwtService _jwtService;

    public GetCalendarEventsUseCaseManager(
        ICalendarEventService calendarEventService,
        IJwtService jwtService)
    {
        _calendarEventService = calendarEventService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> Execute(CancellationToken cancellationToken)
    {
        var currentUser = _jwtService.GetCurrentUser();

        return await _calendarEventService.GetByUserId(
            currentUser.UserId,
            cancellationToken);
    }
}