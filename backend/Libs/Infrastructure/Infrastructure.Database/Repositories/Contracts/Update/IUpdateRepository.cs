using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Database.Repositories.Contracts.Update;

/// <summary>
/// Базовый репозиторий обновления
/// </summary>
public interface IUpdateRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{
    /// <summary>
    /// Обновление сущности
    /// </summary>
    Task UpdateAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Обновление сущности
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}