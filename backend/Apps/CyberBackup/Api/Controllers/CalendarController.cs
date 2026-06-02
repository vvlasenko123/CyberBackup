using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Application.DTO.Calendar;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер календаря
/// </summary>
[ApiController]
[Route("calendar")]
[Authorize]
public sealed class CalendarController : PublicController
{
    private readonly IMapper _mapper;
    private readonly ICreateCalendarEventUseCaseManager _createCalendarEventUseCaseManager;
    private readonly IGetCalendarEventsUseCaseManager _getCalendarEventsUseCaseManager;
    private readonly ICalendarEventService _calendarEventService;
    private readonly IJwtService _jwtService;

    public CalendarController(
        IMapper mapper,
        ICreateCalendarEventUseCaseManager createCalendarEventUseCaseManager,
        IGetCalendarEventsUseCaseManager getCalendarEventsUseCaseManager,
        ICalendarEventService calendarEventService,
        IJwtService jwtService)
    {
        _mapper = mapper;
        _createCalendarEventUseCaseManager = createCalendarEventUseCaseManager;
        _getCalendarEventsUseCaseManager = getCalendarEventsUseCaseManager;
        _calendarEventService = calendarEventService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Создать событие календаря
    /// </summary>
    [HttpPost("event")]
    public async Task<IActionResult> CreateEvent(CreateCalendarEventRequest request, CancellationToken token)
    {
        var dto = _mapper.Map<CalendarEventDto>(request);

        var calendarEvent = await _createCalendarEventUseCaseManager.Execute(dto, token);

        var response = _mapper.Map<CalendarEventResponse>(calendarEvent);

        return Ok(response);
    }

    /// <summary>
    /// Получить события календаря текущего пользователя
    /// </summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(CancellationToken token)
    {
        var events = await _getCalendarEventsUseCaseManager.Execute(token);
        var response = _mapper.Map<IReadOnlyCollection<CalendarEventResponse>>(events);

        return Ok(response);
    }

    /// <summary>
    /// Удалить событие календаря
    /// </summary>
    [HttpDelete("event/{id:guid}")]
    [Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var isAdmin = currentUser.Role is Domain.User.Enums.UserRole.Admin or Domain.User.Enums.UserRole.SuperAdmin;
        await _calendarEventService.DeleteAsync(id, currentUser.UserId, isAdmin, token);
        return NoContent();
    }
}