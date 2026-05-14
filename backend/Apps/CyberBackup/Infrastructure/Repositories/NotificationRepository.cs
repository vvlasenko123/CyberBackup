using Domain.Calendar;
using Domain.Repositories;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <summary>
/// Репозиторий уведомлений.
/// </summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly IAsyncDbConnection _connection;

    public NotificationRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task CreateAsync(NotificationModel notification, CancellationToken cancellationToken)
    {
        const string sql = """
                               INSERT INTO notifications (
                                   id,
                                   user_id,
                                   calendar_event_id,
                                   title,
                                   message,
                                   is_read,
                                   created_at_utc
                               )
                               VALUES (
                                   @Id,
                                   @UserId,
                                   @CalendarEventId,
                                   @Title,
                                   @Message,
                                   @IsRead,
                                   @CreatedAtUtc
                               )
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            notification.Id,
            notification.UserId,
            notification.CalendarEventId,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAtUtc
        }, cancellationToken);
    }
}