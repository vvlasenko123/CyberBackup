using Infrastructure.Database.Migrations.Aggregate;

namespace Infrastructure.Database.Migrations.Contracts;

/// <summary>
/// Репозиторий для работы с состоянием миграций базы данных
/// </summary>
public interface IMigrationRepository
{
    /// <summary>
    /// Применение миграций
    /// </summary>
    Task<IReadOnlyCollection<Migration>> GetAllMigrations(CancellationToken cancellationToken);

    /// <summary>
    /// Получение последней примененной миграции
    /// </summary>
    Task<Migration?> GetLatestAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Применение миграций
    /// </summary>
    Task MigrateUpAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Откат последней миграции
    /// </summary>
    Task MigrateDownAsync(CancellationToken cancellationToken);
}