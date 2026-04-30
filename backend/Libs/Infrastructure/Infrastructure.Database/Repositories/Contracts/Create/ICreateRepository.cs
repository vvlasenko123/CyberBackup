using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Database.Repositories.Contracts.Create;

/// <summary>
/// Базовый репозиторий создания
/// </summary>
public interface ICreateRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{
    /// <summary>
    /// Добавление сущности
    /// </summary>
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Добавление набора сущностей
    /// </summary>
    Task CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}