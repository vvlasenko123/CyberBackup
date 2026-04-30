using Dapper;
using Infrastructure.Database.Additional;
using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;
using Infrastructure.Database.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.HostedService;

/// <summary>
/// Фоновый сервис применения миграций Postgres, вместо fluent будем определять по контракту
/// </summary>
public sealed class PostgresAutoMigrationHostedService : IHostedService
{
    /// <summary>
    /// Провайдер
    /// </summary>
    private readonly IServiceProvider _serviceProvider;
    
    /// <summary>
    /// опции
    /// </summary>
    private readonly IOptions<PostgresOptions> _options;
    
    /// <summary>
    /// логгер
    /// </summary>
    private readonly ILogger<PostgresAutoMigrationHostedService> _logger;

    /// <summary>
    /// Соединение с бд
    /// </summary>
    private readonly IAsyncDbConnection _connection;

    /// <summary>
    /// Создатель бд
    /// </summary>
    private readonly PostgresDatabaseCreator _databaseCreator;

    public PostgresAutoMigrationHostedService(
        IServiceProvider serviceProvider,
        IOptions<PostgresOptions> options,
        ILogger<PostgresAutoMigrationHostedService> logger,
        IAsyncDbConnection connection,
        PostgresDatabaseCreator databaseCreator)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        _connection = connection;
        _databaseCreator = databaseCreator;
    }

    /// <summary>
    /// старт миграций
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Проверка существования базы данных");

        await _databaseCreator.CreatedAsync(cancellationToken);

        _logger.LogInformation("База данных готова");

        if (_options.Value.AutoMigration is false)
        {
            _logger.LogInformation("Автоматическое применение миграций отключено");
            return;
        }

        _logger.LogInformation("Начато применение миграций Postgres");

        using (var scope = _serviceProvider.CreateScope())
        {
            var connection = await _connection.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS migrations (
                    id TEXT PRIMARY KEY,
                    applied_at TIMESTAMPTZ NOT NULL
                );
            """);

            var applied = (await connection.QueryAsync<string>(
                "SELECT id FROM migrations"))
                .ToHashSet();

            var migrations = scope.ServiceProvider
                .GetServices<IDatabaseMigration>()
                .OrderBy(x => x.Id)
                .ToList();

            if (!migrations.Any())
            {
                _logger.LogInformation("Миграции Postgres не найдены");
                return;
            }

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

                _logger.LogInformation("Применение миграции {MigrationId}", migration.Id);

                await migration.MigrateUp(cancellationToken);

                await connection.ExecuteAsync(
                    "INSERT INTO migrations (id, applied_at) VALUES (@Id, now())",
                    new { migration.Id });
            }
        }

        _logger.LogInformation("Применение миграций завершено");
    }

    /// <summary>
    /// стоп миграциям
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
