using Application.DTO.Calendar;

namespace Application.Abstractions.Services.Calendar.Contracts;

/// <summary>
/// Сервис отправки уведомлений на сайт
/// </summary>
public interface INotificationPushService
{
    /// <summary>
    /// Отправить уведомление пользователю
    /// </summary>
    Task SendToUserAsync(Guid userId, NotificationMessageDto notification, CancellationToken cancellationToken);
}