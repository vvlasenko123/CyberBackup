using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания событий календаря
/// </summary>
internal sealed class CreateCalendarEvents_202605150001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateCalendarEvents_202605150001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateCalendarEvents_202605150001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS calendar_events (
                                   id UUID PRIMARY KEY,
                                   user_id UUID NOT NULL,
                                   title VARCHAR(255) NOT NULL,
                                   status INTEGER NOT NULL,
                                   starts_at_utc TIMESTAMPTZ NOT NULL,
                                   ends_at_utc TIMESTAMPTZ NOT NULL,
                                   notify_at_utc TIMESTAMPTZ NULL,
                                   notified_at_utc TIMESTAMPTZ NULL,
                                   created_at_utc TIMESTAMPTZ NOT NULL,
                                   updated_at_utc TIMESTAMPTZ NOT NULL,

                                   CONSTRAINT fk_calendar_events_users
                                       FOREIGN KEY (user_id)
                                       REFERENCES users(id)
                                       ON DELETE CASCADE
                               );

                               CREATE INDEX IF NOT EXISTS ix_calendar_events_user_id
                                   ON calendar_events(user_id);

                               CREATE INDEX IF NOT EXISTS ix_calendar_events_starts_at_utc
                                   ON calendar_events(starts_at_utc);

                               CREATE INDEX IF NOT EXISTS ix_calendar_events_notify_at_utc
                                   ON calendar_events(notify_at_utc)
                                   WHERE notify_at_utc IS NOT NULL
                                     AND notified_at_utc IS NULL;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS calendar_events;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}