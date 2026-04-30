using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Database.Repositories.Contracts.Get;

/// <summary>
/// Базовый репозиторий получения
/// </summary>
public interface IGetRepository<TEntity, in TType> where TEntity : IAggregateRoot<TType>
{
    /// <summary>
    /// Получение сущности по идентификатору
    /// </summary>
    Task<TEntity?> GetByIdAsync(TType id, CancellationToken cancellationToken)
    {
        return Task.FromResult<TEntity?>(default);
    }
    
    /// <summary>
    /// Получение первого элемента
    /// </summary>
    Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<TEntity?>(default);
    }

    /// <summary>
    /// Получение первого элемента или null
    /// </summary>
    Task<TEntity?> GetLatestAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<TEntity?>(default);
    }

    /// <summary>
    /// Получение списка сущностей
    /// </summary>
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<TEntity>>([]);
    }

    /// <summary>
    /// Получение списка сущностей по идентификаторам
    /// </summary>
    Task<IReadOnlyCollection<TEntity>> GetByIdsAsync(IEnumerable<TType> ids, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<TEntity>>([]);
    }

    /// <summary>
    /// Проверка существования сущности
    /// </summary>
    Task<bool> ExistsAsync(TType id, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}