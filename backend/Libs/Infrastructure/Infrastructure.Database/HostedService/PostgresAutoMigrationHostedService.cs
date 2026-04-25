using Infrastructure.Database.Base;
using Infrastructure.Database.HostedService.Contracts;
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
    /// Создатель бд
    /// </summary>
    private readonly PostgresDatabaseCreator _databaseCreator;

    public PostgresAutoMigrationHostedService(
        IServiceProvider serviceProvider,
        IOptions<PostgresOptions> options,
        ILogger<PostgresAutoMigrationHostedService> logger,
        PostgresDatabaseCreator databaseCreator)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        _databaseCreator = databaseCreator;
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
            var migrations = scope.ServiceProvider.GetServices<IDatabaseMigration>().ToList();

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

                await migration.ApplyAsync(cancellationToken);
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
