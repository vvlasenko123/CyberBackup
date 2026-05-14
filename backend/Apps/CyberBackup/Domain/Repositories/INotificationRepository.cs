using Domain.Calendar;

namespace Domain.Repositories;

/// <summary>
/// Репозиторий уведомлений
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Создать уведомление
    /// </summary>
    Task CreateAsync(NotificationModel notification, CancellationToken cancellationToken);
}