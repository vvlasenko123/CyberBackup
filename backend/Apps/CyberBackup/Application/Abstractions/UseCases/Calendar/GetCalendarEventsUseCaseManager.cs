using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Domain.Calendar;
using Domain.User.Enums;

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

        return currentUser.Role switch
        {
            UserRole.Admin or UserRole.SuperAdmin =>
                await _calendarEventService.GetAll(cancellationToken),
            UserRole.Teacher =>
                await _calendarEventService.GetTeacherEvents(currentUser.UserId, cancellationToken),
            _ =>
                await _calendarEventService.GetStudentEvents(currentUser.UserId, cancellationToken),
        };
    }
}