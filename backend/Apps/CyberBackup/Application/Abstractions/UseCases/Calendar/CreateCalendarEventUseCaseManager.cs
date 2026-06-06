using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Application.DTO.Calendar;
using Domain.Calendar;

namespace Application.Abstractions.UseCases.Calendar;

/// <inheritdoc />
public sealed class CreateCalendarEventUseCaseManager : ICreateCalendarEventUseCaseManager
{
    private readonly ICalendarEventService _calendarEventService;
    private readonly IJwtService _jwtService;

    public CreateCalendarEventUseCaseManager(
        ICalendarEventService calendarEventService,
        IJwtService jwtService)
    {
        _calendarEventService = calendarEventService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task<CalendarEventModel> Execute(CalendarEventDto request, CancellationToken cancellationToken)
    {
        var currentUser = _jwtService.GetCurrentUser();

        return await _calendarEventService.Create(request, currentUser.UserId, currentUser.Role, cancellationToken);
    }
}