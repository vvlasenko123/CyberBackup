using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Database.Migrations;

/// <summary>
/// Миграция создания групп
/// </summary>
internal sealed class CreateGroups_202604301230 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateGroups_202604301230(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateGroups_202604301230);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS groups (
                                   id UUID PRIMARY KEY,
                                   name VARCHAR(255) NOT NULL,
                                   created_at TIMESTAMPTZ NOT NULL
                               );
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS groups;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}