namespace Infrastructure.Database.Migrations.Contracts;

/// <summary>
/// Контракт миграций, сделаем свое, fluent мне не нравится
/// </summary>
public interface IDatabaseMigration
{
    /// <summary>
    /// Применение миграций
    /// </summary>
    Task MigrateUp(CancellationToken token);

    /// <summary>
    /// Откат миграций
    /// </summary>
    Task MigrateDown(CancellationToken token);
}