using Application.Abstractions.Services.Calendar.Contracts;
using Application.DTO.Calendar;
using Domain.Calendar;
using Domain.Calendar.Enums;
using Domain.Repositories;

namespace Application.Abstractions.Services.Calendar;

/// <inheritdoc />
public sealed class CalendarEventService : ICalendarEventService
{
    private readonly ICalendarEventRepository _calendarEventRepository;

    public CalendarEventService(ICalendarEventRepository calendarEventRepository)
    {
        _calendarEventRepository = calendarEventRepository;
    }

    /// <inheritdoc />
    public async Task<CalendarEventModel> Create(CalendarEventDto request, Guid userId, CancellationToken cancellationToken)
    {
        Validate(request);

        var nowUtc = DateTimeOffset.UtcNow;

        var calendarEvent = new CalendarEventModel(
            id: UUIDNext.Uuid.NewSequential(),
            userId: userId,
            title: request.Title.Trim(),
            eventType: request.EventType,
            status: CalendarEventStatus.Active,
            startsAtUtc: request.StartsAtUtc.ToUniversalTime(),
            endsAtUtc: request.EndsAtUtc.ToUniversalTime(),
            notifyAtUtc: request.NotifyAtUtc?.ToUniversalTime(),
            notifiedAtUtc: null,
            createdAtUtc: nowUtc,
            updatedAtUtc: nowUtc);

        await _calendarEventRepository.CreateAsync(calendarEvent, cancellationToken);

        return calendarEvent;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return await _calendarEventRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetAll(CancellationToken cancellationToken)
    {
        return await _calendarEventRepository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return _calendarEventRepository.DeleteByIdAsync(id, cancellationToken);
        }

        return _calendarEventRepository.DeleteAsync(id, userId, cancellationToken);
    }

    /// <summary>
    /// Проверить событие календаря
    /// </summary>
    private static void Validate(CalendarEventDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Название события не должно быть пустым");
        }

        if (request.EndsAtUtc <= request.StartsAtUtc)
        {
            throw new InvalidOperationException("Дата окончания события должна быть больше даты начала");
        }

        if (request.NotifyAtUtc is not null && request.NotifyAtUtc > request.StartsAtUtc)
        {
            throw new InvalidOperationException("Дата уведомления не может быть позже даты начала события");
        }
    }
}