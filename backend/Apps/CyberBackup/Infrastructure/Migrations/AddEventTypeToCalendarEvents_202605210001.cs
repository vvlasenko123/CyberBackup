using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция добавления типа события в календарь
/// </summary>
internal sealed class AddEventTypeToCalendarEvents_202605210001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public AddEventTypeToCalendarEvents_202605210001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(AddEventTypeToCalendarEvents_202605210001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               ALTER TABLE calendar_events
                                   ADD COLUMN IF NOT EXISTS event_type INTEGER NOT NULL DEFAULT 0;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               ALTER TABLE calendar_events DROP COLUMN IF EXISTS event_type;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
