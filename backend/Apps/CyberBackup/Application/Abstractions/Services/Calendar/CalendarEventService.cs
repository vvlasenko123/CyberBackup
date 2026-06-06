using Application.Abstractions.Services.Calendar.Contracts;
using Application.DTO.Calendar;
using Domain.Calendar;
using Domain.Calendar.Enums;
using Domain.Repositories;
using Domain.User.Enums;

namespace Application.Abstractions.Services.Calendar;

/// <inheritdoc />
public sealed class CalendarEventService : ICalendarEventService
{
    private readonly ICalendarEventRepository _calendarEventRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPushService _notificationPush;

    public CalendarEventService(
        ICalendarEventRepository calendarEventRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        INotificationPushService notificationPush)
    {
        _calendarEventRepository = calendarEventRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _notificationPush = notificationPush;
    }

    /// <inheritdoc />
    public async Task<CalendarEventModel> Create(CalendarEventDto request, Guid userId, UserRole creatorRole, CancellationToken cancellationToken)
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

        try
        {
            await SendCreationNotificationsAsync(calendarEvent, userId, creatorRole, cancellationToken);
        }
        catch
        {
            // Уведомления — best-effort, не ломают создание события
        }

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
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetStudentEvents(Guid studentId, CancellationToken cancellationToken)
    {
        return await _calendarEventRepository.GetStudentEventsAsync(studentId, cancellationToken);
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

    private async Task SendCreationNotificationsAsync(
        CalendarEventModel calendarEvent,
        Guid creatorId,
        UserRole creatorRole,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> recipientIds;

        if (creatorRole is UserRole.Admin or UserRole.SuperAdmin)
        {
            recipientIds = await _userRepository.GetUserIdsByRolesAsync(
                [(int)UserRole.Student, (int)UserRole.Teacher],
                cancellationToken);
        }
        else if (creatorRole == UserRole.Teacher)
        {
            recipientIds = await _userRepository.GetStudentIdsByTeacherAsync(creatorId, cancellationToken);
        }
        else
        {
            return;
        }

        var title = "Новое событие в календаре";
        var message = calendarEvent.Title;
        var nowUtc = DateTimeOffset.UtcNow;

        foreach (var recipientId in recipientIds)
        {
            if (recipientId == creatorId) continue;

            var notification = new NotificationModel(
                id: UUIDNext.Uuid.NewSequential(),
                userId: recipientId,
                calendarEventId: calendarEvent.Id,
                title: title,
                message: message,
                isRead: false,
                createdAtUtc: nowUtc);

            await _notificationRepository.CreateAsync(notification, cancellationToken);

            var dto = new NotificationMessageDto(notification.Id, title, message, nowUtc);
            await _notificationPush.SendToUserAsync(recipientId, dto, cancellationToken);
        }
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