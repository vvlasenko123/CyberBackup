using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания таблицы постов (лента новостей)
/// </summary>
internal sealed class CreatePosts_202605270001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreatePosts_202605270001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreatePosts_202605270001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS posts (
                               id UUID PRIMARY KEY,
                               author_id UUID NOT NULL,
                               title VARCHAR(255) NOT NULL,
                               content VARCHAR(2000) NOT NULL,
                               category INT NOT NULL,
                               created_at_utc TIMESTAMPTZ NOT NULL,

                               CONSTRAINT fk_posts_users
                                   FOREIGN KEY (author_id)
                                   REFERENCES users(id)
                                   ON DELETE CASCADE
                           );

                           CREATE INDEX IF NOT EXISTS ix_posts_category ON posts(category);
                           CREATE INDEX IF NOT EXISTS ix_posts_created_at_utc ON posts(created_at_utc DESC);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = "DROP TABLE IF EXISTS posts;";
        await _connection.ExecuteAsync(sql, null, token);
    }
}
