using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Core.Repositories.Contracts.Get;

/// <summary>
/// Базовый репозиторий получения
/// </summary>
public interface IGetRepository<TEntity, in TType> where TEntity : IAggregateRoot<TType>
{
}