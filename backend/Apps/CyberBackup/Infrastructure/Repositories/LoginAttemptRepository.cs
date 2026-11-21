using Dapper;
using Domain.Auth;
using Domain.Repositories;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class LoginAttemptRepository : ILoginAttemptRepository
{
    /// <summary>
    /// Записи старше суток без активной блокировки удаляются: иначе случайные почты разрастили бы таблицу
    /// </summary>
    private static readonly TimeSpan StaleAfter = TimeSpan.FromHours(24);

    private readonly IAsyncDbConnection _connection;

    public LoginAttemptRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        string key,
        Func<LoginAttemptState?, LoginAttemptState> update,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // Строка создаётся пустой, чтобы FOR UPDATE было что блокировать даже для нового ключа
        const string ensureSql = """
                                 INSERT INTO login_attempts (attempt_key, failed_count, last_attempt_at_utc)
                                 VALUES (@Key, 0, to_timestamp(0))
                                 ON CONFLICT (attempt_key) DO NOTHING;
                                 """;

        const string selectSql = """
                                 SELECT
                                     failed_count AS FailedCount,
                                     last_attempt_at_utc AS LastAttemptAtUtc,
                                     locked_until_utc AS LockedUntilUtc
                                 FROM login_attempts
                                 WHERE attempt_key = @Key
                                 FOR UPDATE;
                                 """;

        const string updateSql = """
                                 UPDATE login_attempts
                                 SET failed_count = @FailedCount,
                                     last_attempt_at_utc = @LastAttemptAtUtc,
                                     locked_until_utc = @LockedUntilUtc
                                 WHERE attempt_key = @Key;
                                 """;

        const string cleanupSql = """
                                  DELETE FROM login_attempts
                                  WHERE last_attempt_at_utc < @Cutoff
                                    AND attempt_key <> @Key
                                    AND (locked_until_utc IS NULL OR locked_until_utc < @Cutoff);
                                  """;

        await connection.ExecuteAsync(ensureSql, new { Key = key }, transaction);

        var row = await connection.QuerySingleAsync<AttemptRow>(selectSql, new { Key = key }, transaction);
        var current = row.FailedCount == 0
            ? null
            : new LoginAttemptState(row.FailedCount, row.LastAttemptAtUtc, row.LockedUntilUtc);

        var next = update(current);

        await connection.ExecuteAsync(updateSql, new
        {
            Key = key,
            next.FailedCount,
            next.LastAttemptAtUtc,
            next.LockedUntilUtc
        }, transaction);

        if (current is null)
        {
            await connection.ExecuteAsync(
                cleanupSql,
                new { Key = key, Cutoff = DateTimeOffset.UtcNow - StaleAfter },
                transaction);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        await _connection.ExecuteAsync(
            "DELETE FROM login_attempts WHERE attempt_key = @Key;",
            new { Key = key },
            cancellationToken);
    }

    private sealed class AttemptRow
    {
        public int FailedCount { get; init; }

        public DateTimeOffset LastAttemptAtUtc { get; init; }

        public DateTimeOffset? LockedUntilUtc { get; init; }
    }
}
