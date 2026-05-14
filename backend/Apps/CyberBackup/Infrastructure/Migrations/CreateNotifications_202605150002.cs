using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания уведомлений
/// </summary>
internal sealed class CreateNotifications_202605150002 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateNotifications_202605150002(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateNotifications_202605150002);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS notifications (
                                   id UUID PRIMARY KEY,
                                   user_id UUID NOT NULL,
                                   calendar_event_id UUID NULL,
                                   title VARCHAR(255) NOT NULL,
                                   message VARCHAR(1000) NOT NULL,
                                   is_read BOOLEAN NOT NULL,
                                   created_at_utc TIMESTAMPTZ NOT NULL,

                                   CONSTRAINT fk_notifications_users
                                       FOREIGN KEY (user_id)
                                       REFERENCES users(id)
                                       ON DELETE CASCADE,

                                   CONSTRAINT fk_notifications_calendar_events
                                       FOREIGN KEY (calendar_event_id)
                                       REFERENCES calendar_events(id)
                                       ON DELETE CASCADE
                               );

                               CREATE INDEX IF NOT EXISTS ix_notifications_user_id
                                   ON notifications(user_id);

                               CREATE INDEX IF NOT EXISTS ix_notifications_is_read
                                   ON notifications(is_read);

                               CREATE INDEX IF NOT EXISTS ix_notifications_created_at_utc
                                   ON notifications(created_at_utc);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS notifications;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}