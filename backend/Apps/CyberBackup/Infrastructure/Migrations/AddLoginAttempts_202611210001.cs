using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Таблица счётчиков неверных попыток ввода пароля (защита от перебора)
/// </summary>
internal sealed class AddLoginAttempts_202611210001(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddLoginAttempts_202611210001);

    public Task MigrateUp(CancellationToken token) => connection.ExecuteAsync("""
        CREATE TABLE IF NOT EXISTS login_attempts (
            attempt_key VARCHAR(64) PRIMARY KEY,
            failed_count INT NOT NULL,
            last_attempt_at_utc TIMESTAMPTZ NOT NULL,
            locked_until_utc TIMESTAMPTZ NULL
        );
        CREATE INDEX IF NOT EXISTS ix_login_attempts_last_attempt_at_utc ON login_attempts(last_attempt_at_utc);
        """, null, token);

    public Task MigrateDown(CancellationToken token) => connection.ExecuteAsync("""
        DROP TABLE IF EXISTS login_attempts;
        """, null, token);
}
