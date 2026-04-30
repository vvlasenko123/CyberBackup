using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Database.Migrations;

/// <summary>
/// Миграция создания пользователей
/// </summary>
internal sealed class CreateUsers_202604301231 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateUsers_202604301231(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateUsers_202604301231);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS users (
                                   id UUID PRIMARY KEY,
                                   email VARCHAR(255) NOT NULL,
                                   password VARCHAR(255) NOT NULL,
                                   full_name VARCHAR(255) NOT NULL,
                                   role INTEGER NOT NULL,
                                   is_active BOOLEAN NOT NULL,
                                   must_change_password BOOLEAN NOT NULL,
                                   created_by UUID NULL,
                                   created_at TIMESTAMPTZ NOT NULL,
                                   updated_at TIMESTAMPTZ NOT NULL
                               );
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS users;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}