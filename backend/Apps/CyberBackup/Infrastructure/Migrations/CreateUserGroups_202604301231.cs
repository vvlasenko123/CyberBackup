using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция связи пользователей и групп
/// </summary>
internal sealed class CreateUserGroups_202604301231 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateUserGroups_202604301231(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateUserGroups_202604301231);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS user_groups (
                                   user_id UUID NOT NULL,
                                   group_id UUID NOT NULL
                               );
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS user_groups;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}