using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания refresh token.
/// </summary>
internal sealed class CreateRefreshTokens_202605030001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateRefreshTokens_202605030001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateRefreshTokens_202605030001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                               CREATE TABLE IF NOT EXISTS refresh_tokens (
                                   id UUID PRIMARY KEY,
                                   user_id UUID NOT NULL,
                                   token_hash VARCHAR(512) NOT NULL,
                                   session_id UUID NOT NULL,
                                   created_at_utc TIMESTAMPTZ NOT NULL,
                                   expires_at_utc TIMESTAMPTZ NOT NULL,
                                   revoked_at_utc TIMESTAMPTZ NULL,
                                   replaced_by_token_id UUID NULL,

                                   CONSTRAINT fk_refresh_tokens_users
                                       FOREIGN KEY (user_id)
                                       REFERENCES users(id)
                                       ON DELETE CASCADE,

                                   CONSTRAINT fk_refresh_tokens_replaced_by
                                       FOREIGN KEY (replaced_by_token_id)
                                       REFERENCES refresh_tokens(id)
                                       ON DELETE SET NULL
                               );

                               CREATE UNIQUE INDEX IF NOT EXISTS ux_refresh_tokens_token_hash
                                   ON refresh_tokens(token_hash);

                               CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user_id
                                   ON refresh_tokens(user_id);

                               CREATE INDEX IF NOT EXISTS ix_refresh_tokens_session_id
                                   ON refresh_tokens(session_id);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                               DROP TABLE IF EXISTS refresh_tokens;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}