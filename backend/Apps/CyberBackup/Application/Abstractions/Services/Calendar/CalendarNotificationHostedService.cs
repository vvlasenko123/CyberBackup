using Application.Abstractions.Services.Calendar.Contracts;
using Application.DTO.Calendar;
using Domain.Calendar;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Calendar;

/// <summary>
/// Фоновый сервис уведомлений календаря
/// </summary>
public sealed class CalendarNotificationHostedService : BackgroundService
{
    private const int DelayInSeconds = 60;
    private const int BatchSize = 100;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CalendarNotificationHostedService> _logger;

    public CalendarNotificationHostedService(
        IServiceProvider serviceProvider,
        ILogger<CalendarNotificationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNotifications(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка обработки уведомлений календаря");
            }

            await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds), stoppingToken);
        }
    }

    /// <summary>
    /// Обработать уведомления
    /// </summary>
    private async Task ProcessNotifications(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var calendarEventRepository = scope.ServiceProvider.GetRequiredService<ICalendarEventRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var notificationPushService = scope.ServiceProvider.GetRequiredService<INotificationPushService>();

        var nowUtc = DateTimeOffset.UtcNow;

        var calendarEventList = await calendarEventRepository.GetForNotificationAsync(
            nowUtc,
            BatchSize,
            cancellationToken);

        foreach (var calendarEvent in calendarEventList)
        {
            var notification = new NotificationModel(
                id: UUIDNext.Uuid.NewSequential(),
                userId: calendarEvent.UserId,
                calendarEventId: calendarEvent.Id,
                title: "Напоминание о событии",
                message: $"Скоро начнётся событие: {calendarEvent.Title}",
                isRead: false,
                createdAtUtc: nowUtc);

            await notificationRepository.CreateAsync(
                notification,
                cancellationToken);

            await notificationPushService.SendToUserAsync(
                calendarEvent.UserId,
                new NotificationMessageDto(
                    Id: notification.Id,
                    Title: notification.Title,
                    Message: notification.Message,
                    CreatedAtUtc: notification.CreatedAtUtc),
                cancellationToken);

            await calendarEventRepository.SetNotifiedAsync(
                calendarEvent.Id,
                nowUtc,
                cancellationToken);
        }

        if (calendarEventList.Count > 0)
        {
            _logger.LogInformation(
                "Создано уведомлений календаря: {Count}",
                calendarEventList.Count);
        }
    }
}