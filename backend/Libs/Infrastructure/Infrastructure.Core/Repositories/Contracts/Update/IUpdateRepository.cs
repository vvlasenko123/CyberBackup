using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Core.Repositories.Contracts.Update;

/// <summary>
/// Базовый репозиторий обновления
/// </summary>
public interface IUpdateRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{
}