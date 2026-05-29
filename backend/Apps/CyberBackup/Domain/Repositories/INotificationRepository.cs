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

    /// <summary>
    /// Получить последние уведомления пользователя
    /// </summary>
    Task<IReadOnlyCollection<NotificationModel>> GetForUserAsync(Guid userId, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Пометить все уведомления пользователя как прочитанные
    /// </summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken);
}