using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Aggregate;
using Infrastructure.Database.Migrations.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.Migrations.Repository;

/// <summary>
/// Репозиторий для работы с миграциями
/// </summary>
public sealed class MigrationRepository : IMigrationRepository
{
    /// <summary>
    /// Соединение с базой данных
    /// </summary>
    private readonly IAsyncDbConnection _connection;

    /// <summary>
    /// Провайдер сервисов
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    public MigrationRepository(
        IAsyncDbConnection connection,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Migration>> GetAllMigrations(CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT 
                                   id AS "Id",
                                   applied_at AS "AppliedAt"
                               FROM migrations
                               ORDER BY applied_at
                           """;

        return await _connection.QueryAsync<Migration>(sql, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Migration?> GetLatestAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT 
                                   id AS Id,
                                   applied_at AS AppliedAt
                               FROM migrations
                               ORDER BY applied_at DESC
                               LIMIT 1
                           """;

        return await _connection.QueryFirstOrDefaultAsync<Migration>(
            sql,
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM migrations
            WHERE id = @Id
        """;

        await _connection.ExecuteAsync(sql, new { Id = id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MigrateUpAsync(CancellationToken cancellationToken)
    {
        const string createTableSql = """
                                          CREATE TABLE IF NOT EXISTS migrations (
                                              id TEXT PRIMARY KEY,
                                              applied_at TIMESTAMPTZ NOT NULL
                                          );
                                      """;

        await _connection.ExecuteAsync(createTableSql, null, cancellationToken);

        var applied = (await _connection.QueryAsync<string>(
            "SELECT id FROM migrations",
            null,
            cancellationToken)).ToHashSet();

        var migrations = _serviceProvider
            .GetServices<IDatabaseMigration>()
            .ToList();

        foreach (var migration in migrations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (applied.Contains(migration.Id))
            {
                continue;
            }

            await migration.MigrateUp(cancellationToken);

            await _connection.ExecuteAsync(
                "INSERT INTO migrations (id, applied_at) VALUES (@Id, now())",
                new { migration.Id },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task MigrateDownAsync(CancellationToken cancellationToken)
    {
        var latest = await GetLatestAsync(cancellationToken);

        if (latest is null)
        {
            return;
        }

        var migrations = _serviceProvider.GetServices<IDatabaseMigration>();

        var migration = migrations
            .FirstOrDefault(x => x.Id == latest.Id);

        if (migration is null)
        {
            throw new InvalidOperationException($"Миграция {latest.Id} не найдена");
        }

        await migration.MigrateDown(cancellationToken);

        await _connection.ExecuteAsync(
            "DELETE FROM migrations WHERE id = @Id",
            new { latest.Id },
            cancellationToken);
    }
}
