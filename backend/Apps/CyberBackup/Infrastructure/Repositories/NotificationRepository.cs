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

    /// <inheritdoc />
    public Task<IReadOnlyCollection<NotificationModel>> GetForUserAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id             AS "Id",
                                   user_id        AS "UserId",
                                   calendar_event_id AS "CalendarEventId",
                                   title          AS "Title",
                                   message        AS "Message",
                                   is_read        AS "IsRead",
                                   created_at_utc AS "CreatedAtUtc"
                               FROM notifications
                               WHERE user_id = @UserId
                               ORDER BY created_at_utc DESC
                               LIMIT @Limit
                           """;

        return _connection.QueryAsync<NotificationModel>(sql, new { UserId = userId, Limit = limit }, cancellationToken);
    }

    /// <inheritdoc />
    public Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
                               UPDATE notifications
                               SET is_read = TRUE
                               WHERE user_id = @UserId AND is_read = FALSE
                           """;

        return _connection.ExecuteAsync(sql, new { UserId = userId }, cancellationToken);
    }
}