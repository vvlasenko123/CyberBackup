using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Application.DTO.Calendar;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public CalendarController(
        IMapper mapper,
        ICreateCalendarEventUseCaseManager createCalendarEventUseCaseManager,
        IGetCalendarEventsUseCaseManager getCalendarEventsUseCaseManager)
    {
        _mapper = mapper;
        _createCalendarEventUseCaseManager = createCalendarEventUseCaseManager;
        _getCalendarEventsUseCaseManager = getCalendarEventsUseCaseManager;
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
}