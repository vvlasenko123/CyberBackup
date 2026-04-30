using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Database.Repositories.Contracts.Delete;

/// <summary>
/// Базовый репозиторий удаления
/// </summary>
public interface IDeleteRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{
    /// <summary>
    /// Удаление по идентификатору
    /// </summary>
    Task DeleteAsync(TType id, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаление сущности
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаление набора сущностей по идентификаторам
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TType> ids, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаление набора сущностей
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}